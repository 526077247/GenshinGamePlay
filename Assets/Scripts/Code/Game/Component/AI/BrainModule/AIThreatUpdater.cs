using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 威胁模块
    /// </summary>
    public class AIThreatUpdater: BrainModuleBase
    {
        private AIComponent aiComponent;

        //警戒列表
        private readonly Dictionary<long, ThreatInfo> candidateList = new Dictionary<long, ThreatInfo>();
        //威胁列表
        private readonly Dictionary<long, ThreatInfo> threatList = new Dictionary<long, ThreatInfo>();

        private ThreatInfo topThreat = null;
        //current Target
        private ThreatInfo mainTarget = null;

        //强制离开战斗
        private bool forceLeaveCombat = false;

        protected override void InitInternal()
        {
            base.InitInternal();
            knowledge.CombatComponent.afterBeAttack += AfterBeAttack;
            this.aiComponent = knowledge.Entity.GetComponent<AIComponent>();
        }

        protected override void ClearInternal()
        {
            if (knowledge.CombatComponent != null)
                knowledge.CombatComponent.afterBeAttack -= AfterBeAttack;
            base.ClearInternal();
            foreach (var item in candidateList)
            {
                item.Value.Dispose();
            }
            candidateList.Clear();
            threatList.Clear();
            aiComponent = null;
        }

        private void AfterBeAttack(AttackResult result, CombatComponent other)
        {
            var sourcelessHitAttractionRange = knowledge.SensingKnowledge.SourcelessHitAttractionRange;
            if (other.GetParent<Entity>() is Unit unit)
            {
                if (result.IsBullet)
                {
                    var currentPos = knowledge.CurrentPos;
                    float distanceToTarget = Vector3.Distance(currentPos, unit.Position);
                    //无源受击感知范围判断
                    if (distanceToTarget > sourcelessHitAttractionRange) return;
                }

                ExternalAddCandidate(other.Id, unit.Position, ThreatAddReason.Hit, result.FinalRealDamage);
                ExternalAddThreat(other.Id, unit.Position, ThreatAddReason.Hit, result.FinalRealDamage);
                ForceEnterCombat();
            }
        }
        protected override void UpdateMainThreadInternal()
        {
            if (!knowledge.ThreatKnowledge.Config.Enable)
                return;

            UpdateCandidateList();

            UpdateThreatList();

            SelectTarget();

            UpdateAIKnowledges();

        }

        /// <summary>
        /// 外部调用直接增加指定threat仇恨值
        /// </summary>
        /// <param name="targetID"></param>
        /// <param name="pos"></param>
        /// <param name="reason"></param>
        /// <param name="threatIncrementAmount"></param>
        public void ExternalAddThreat(long targetID, Vector3 pos, ThreatAddReason reason, float threatIncrementAmount)
        {
            if (threatList.TryGetValue(targetID, out var target))
            {
                target.IncreaseThreat(threatIncrementAmount);
            }
            else
            {
                ThreatInfo info = ThreatInfo.Create(targetID, pos, reason);
                info.IncreaseThreat(threatIncrementAmount);
                threatList.Add(info.Id, info);
            }
        }

        public void ExternalAddCandidate(long targetID, Vector3 pos, ThreatAddReason reason, float temperatureIncrementAmount)
        {
            //调用AISensingUpdater.CanSignalBeNoticed()方法进行check 是否能加入感知列表
            if (candidateList.TryGetValue(targetID, out var target))
            {
                target.ThreatPos = pos;
                target.IncreaseTemper(temperatureIncrementAmount);
            }
            else
            {
                ThreatInfo info = ThreatInfo.Create(targetID, pos, reason);
                info.IncreaseTemper(temperatureIncrementAmount);
                candidateList.Add(info.Id, info);
            }
        }

        /// <summary>
        /// 更新警戒队列
        /// </summary>
        private void UpdateCandidateList()
        {
            if (forceLeaveCombat)
            {
                forceLeaveCombat = false;
                foreach (var item in candidateList)
                {
                    item.Value.Dispose();
                }
                candidateList.Clear();
                threatList.Clear();
                return;
            }
            UpdateCandidateFromSensibles();
            var timeNow = GameTimerManager.Instance.GetTimeNow();
            var deltaTime = GameTimerManager.Instance.GetDeltaTime();
            foreach (var candidate in candidateList)
            {
                var addReason = candidate.Value.AddReason;
                float distanceToCandidate = Vector3.Distance(candidate.Value.ThreatPos, knowledge.CurrentPos);
                float temperatureAmount;

                switch (addReason)
                {
                    case ThreatAddReason.Vision:
                        candidate.Value.LastSeenTime = timeNow;
                        var viewAttenuation = knowledge.ThreatKnowledge.ViewAttenuation;
                        float viewAttenuationAmount = 1;
                        if(viewAttenuation!=null)
                        {
                            viewAttenuationAmount = viewAttenuation.Evaluate(distanceToCandidate);
                        }
                        //TODO 加上感知随机偏差系数
                        temperatureAmount = knowledge.ThreatKnowledge.ViewThreatGrowRate * deltaTime * viewAttenuationAmount / 1000;
                        candidate.Value.IncreaseTemper(temperatureAmount);
                        break;
                    case ThreatAddReason.Feel:
                        candidate.Value.LastFeelTime = timeNow;
                        var hearAttenuation = knowledge.ThreatKnowledge.HearAttenuation;
                        float hearAttenuationAmount = 1;
                        if(hearAttenuation!=null)
                        {
                            hearAttenuationAmount = hearAttenuation.Evaluate(distanceToCandidate);
                        }
                        //TODO 加上感知随机偏差系数
                        temperatureAmount = knowledge.ThreatKnowledge.FeelThreatGrowRate * deltaTime * hearAttenuationAmount / 1000;
                        candidate.Value.IncreaseTemper(temperatureAmount);
                        break;
                }
            }

            knowledge.Temperature = 0f;
            foreach (var item in candidateList)
            {
                var candidate = item.Value;

                knowledge.Temperature = knowledge.Temperature > candidate.Temperature
                    ? knowledge.Temperature : candidate.Temperature;
            }

            if (knowledge.Temperature < ThreatInfo.TEMPERVAL_ALERT)
            {
                float decreaseTemperatureAmount = knowledge.ThreatKnowledge.Config.ThreatDecreaseSpeed * deltaTime / 1000;
                using (ListComponent<long> temp = ListComponent<long>.Create())
                {
                    foreach (var kv in candidateList)
                    {
                        var candidate = kv.Value;
                        candidate.DecreaseTemper(decreaseTemperatureAmount);
                        if (!ValidateCandidate(candidate))
                        {
                            var key = kv.Key;
                            temp.Add(key);
                        }
                    }
                    for (int i = 0; i < temp.Count; i++)
                    {
                        var key = temp[i];
                        var candidate = candidateList[key];
                        candidateList.Remove(key);
                        threatList.Remove(key);
                        candidate.Dispose();
                    }
                }
            }
        }

        /// <summary>
        /// 从感知模块中更新感知对象的信息
        /// </summary>
        private void UpdateCandidateFromSensibles()
        {
            var sensibles = knowledge.SensingKnowledge.EnemySensibles;

            if (sensibles == null)
                return;

            foreach (var sensible in sensibles)
            {
                //防守区域范围
                if(!knowledge.DefendAreaKnowledge.IsInDefendRange) continue;
                
                float distanceToSensible = sensible.Value.Distance;
                var feelRange = knowledge.SensingKnowledge.FeelRange;
                if (distanceToSensible <= feelRange)//Feel
                {
                    ProcessSensible(sensible.Value, ThreatAddReason.Feel);
                }
                else if (knowledge.SensingKnowledge.EnableVision)//Vision
                {
                    ProcessSensible(sensible.Value, ThreatAddReason.Vision);
                }
            }
        }

        private void UpdateAIKnowledges()
        {
            knowledge.ThreatKnowledge.ReachAwareThisFrame = false;
            knowledge.ThreatKnowledge.ReachAlertThisFrame = false;

            bool threatLevelChange = false;
            
            if ((knowledge.Temperature < ThreatInfo.TEMPERVAL_AWARE) && knowledge.ThreatLevel != ThreatLevel.Unaware)
            {
                threatLevelChange = true;
                
                knowledge.ThreatLevel = ThreatLevel.Unaware;
                
            }

            if ((knowledge.Temperature > ThreatInfo.TEMPERVAL_AWARE
                && knowledge.Temperature < ThreatInfo.TEMPERVAL_ALERT)
                && knowledge.ThreatLevel != ThreatLevel.Aware)
            {
                threatLevelChange = knowledge.ThreatLevel!=ThreatLevel.Aware;

                knowledge.ThreatKnowledge.ReachAwareThisFrame = true;
                knowledge.ThreatLevel = ThreatLevel.Aware;
            }
            
            if (knowledge.Temperature >= ThreatInfo.TEMPERVAL_ALERT && knowledge.ThreatLevel != ThreatLevel.Alert)
            {
                threatLevelChange = knowledge.ThreatLevel!=ThreatLevel.Alert;
                
                knowledge.ThreatKnowledge.ReachAlertThisFrame = true;
                knowledge.ThreatLevel = ThreatLevel.Alert;
            }

            if (threatLevelChange)
            {
                aiComponent.OnThreatLevelChanged?.Invoke(knowledge.ThreatLevelOld, knowledge.ThreatLevel);
                knowledge.Entity?.GetComponent<PoseFSMComponent>()?
                    .SetData(FSMConst.Alertness, (int)knowledge.ThreatLevel);
                knowledge.CombatComponent.SetCombatState(knowledge.ThreatLevel == ThreatLevel.Alert);
            }

            knowledge.ThreatKnowledge.MainThreat = mainTarget;
        }
        
        private bool ValidateCandidate(ThreatInfo candidate)
        {
            if (candidate.Temperature <= ThreatInfo.TEMPERVAL_CLERAR)
                return false;
            return true;
        }


        /// <summary>
        /// 处理感知目标,根据传入的Threat Add Reason 计算对应的增长
        /// </summary>
        private void ProcessSensible(SensibleInfo sensible, ThreatAddReason sourceType)
        {
            if (!candidateList.ContainsKey(sensible.SensibleID))
            {
                ThreatInfo threatInfo = ThreatInfo.Create(sensible.SensibleID, sensible.Position, sourceType);
                threatInfo.ThreatPos = sensible.Position;
                candidateList.TryAdd(threatInfo.Id, threatInfo);
            }
            else
            {
                candidateList[sensible.SensibleID].ThreatPos = sensible.Position;
                candidateList[sensible.SensibleID].AddReason = sourceType;
            }
        }

        /// <summary>
        /// 更新仇恨列表
        /// </summary>
        private void UpdateThreatList()
        {
            if (knowledge.Temperature < ThreatInfo.TEMPERVAL_ALERT)
            {
                if (threatList.Count > 0)
                    threatList.Clear();
                return;
            }
            var deltaTime = GameTimerManager.Instance.GetDeltaTime();
            var decreaseThreat = deltaTime * knowledge.ThreatKnowledge.Config.ThreatDecreaseSpeed / 1000;
            foreach (var kv in threatList)
            {
                if (!ValidateThreat(kv.Value))
                {
                    kv.Value.DecreaseThreat(decreaseThreat);
                }
            }
            foreach (var candidate in candidateList)
            {
                if (threatList.ContainsKey(candidate.Value.Id))
                    continue;
                candidate.Value.IncreaseThreat(candidate.Value.Temperature);
                threatList.Add(candidate.Value.Id, candidate.Value);
            }
            
            using (ListComponent<long> temp = ListComponent<long>.Create())
            {
                foreach (var kv in threatList)
                {
                    if (kv.Value.ThreatValue <= ThreatInfo.THREATVAL_THREATLOST)
                    {
                        temp.Add(kv.Key);
                    }
                }
                for (int i = 0; i < temp.Count; i++)
                {
                    var key = temp[i];
                    InternalRemoveThreat(key);
                }
            }
        }

        private void SelectTarget()
        {
            if (threatList.Count <= 0)
            {
                topThreat = null;
                mainTarget = null;
            }
            else
            {
                bool isThreatTargetChanged = false;

                long oldThreatTargetID = -1;

                topThreat = null;
                foreach (var item in threatList)
                {
                    if (topThreat == null || item.Value.ThreatValue > topThreat.ThreatValue)
                    {
                        topThreat = item.Value;
                    }
                }

                if (mainTarget != topThreat)
                {
                    isThreatTargetChanged = true;
                    oldThreatTargetID = -1;
                    mainTarget = topThreat;
                }

                if (topThreat != null)
                {
                    if (topThreat.ThreatValue > 1.2f * mainTarget.ThreatValue)
                    {
                        oldThreatTargetID = mainTarget.Id;
                        mainTarget = topThreat;
                        isThreatTargetChanged = true;
                    }
                }

                if (isThreatTargetChanged && mainTarget != null)
                {
                    aiComponent.OnThreatTargetChanged?.Invoke(oldThreatTargetID, mainTarget.Id);
                }
            }
        }
        
        /// <summary>
        /// 判断是否依旧满足Threat对象
        /// </summary>
        /// <param name="threatInfo"></param>
        /// <returns></returns>
        private bool ValidateThreat(ThreatInfo threatInfo)
        {
            var sensibles = knowledge.SensingKnowledge.EnemySensibles;
            if (!sensibles.ContainsKey(threatInfo.Id))
            {
                return false;
            }
            var timeNow = GameTimerManager.Instance.GetTimeNow();
            var distanceFromBorn = Vector3.Distance(knowledge.BornPos, threatInfo.ThreatPos);
            var distanceFromSelf = Vector3.Distance(knowledge.Entity.Position, threatInfo.ThreatPos);
            
            //边缘距离限制
            var edgeRange = knowledge.ThreatKnowledge.Config.ClearThreatEdgeDistance;
            //目标距离限制
            var maxTargetDistance = knowledge.ThreatKnowledge.Config.ClearThreatTargetDistance;
            var clearThreatTimerByDistance = knowledge.ThreatKnowledge.Config.ClearThreatTimerByDistance;
            
            //超距, 目标与AI距离 > 目标距离限制 , 目标处于(防守区域 + 边缘距离限制)外
            if (distanceFromSelf > maxTargetDistance || distanceFromBorn > edgeRange)
            {
                if (!threatInfo.LctByFarDistance.IsRunning())
                    threatInfo.LctByFarDistance.Start(timeNow);
                if (threatInfo.LctByFarDistance.IsElapsed(timeNow, clearThreatTimerByDistance))
                {
                    return false;
                }
            }
            else
            {
                threatInfo.LctByFarDistance.Reset(timeNow);
            }
            
            //目标离开防守区域
            var clearThreatTimerByOutOfZone = knowledge.ThreatKnowledge.Config.ClearThreatTimerByTargetOutOfZone;
            if (knowledge.ThreatKnowledge.Config.ClearThreatByTargetOutOfZone)
            {
                var target = knowledge.AIManager.GetUnit(threatInfo.Id);
                if (target == null) return true;
                //超界
                if (!knowledge.DefendAreaKnowledge.CheckInDefendArea(target.Position))
                {
                    if (!threatInfo.LctByOutOfZone.IsRunning())
                        threatInfo.LctByOutOfZone.Start(timeNow);
                    if (threatInfo.LctByOutOfZone.IsElapsed(timeNow, clearThreatTimerByOutOfZone))
                    {
                        return false;
                    }
                }
                else
                {
                    threatInfo.LctByOutOfZone.Reset(timeNow);
                }
            }

            return true;
        }

        /// <summary>
        /// 移除威胁对象
        /// </summary>
        /// <param name="id"></param>
        private void InternalRemoveThreat(long id)
        {
            if (threatList.TryGetValue(id,out var threatInfo))
            {
                if (mainTarget != null)
                    if (id == mainTarget.Id)
                    {
                        mainTarget = null;
                    }
                if (topThreat != null)
                    if (id == topThreat.Id)
                    {
                        topThreat = null;
                    }
                threatList.Remove(id);
                candidateList.Remove(id);
                threatInfo.Dispose();
            }
            else
            {
                Log.Info("Threat List dont contain target id {0}.", id);
            }
        }

        public void ForceLeaveCombat()
        {
            forceLeaveCombat = true;
        }

        public void ForceEnterCombat()
        {
            knowledge.Temperature = ThreatInfo.TEMPERVAL_ALERT;
        }
    }
}