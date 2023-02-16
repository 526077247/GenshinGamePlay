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
        public AIThreatUpdater(AIComponent aiComponent)
        {
            this.aiComponent = aiComponent;
        }
        
         //警戒列表
        private Dictionary<long, ThreatInfo> _candidateList = new Dictionary<long, ThreatInfo>();
        //威胁列表
        private Dictionary<long, ThreatInfo> _threatList = new Dictionary<long, ThreatInfo>();

        private ThreatInfo _topThreat = null;
        //current Target
        private ThreatInfo _mainTarget = null;

        //强制离开战斗
        private bool _forceLeaveCombat = false;

        //TODO
        //private List<ThreatInfo> _disqualifiedCandidates;
        //private List<ThreatInfo> _disqualifiedThreats;

        protected override void UpdateMainThreadInternal()
        {
            if (!knowledge.threatKnowledge.config.enable)
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
        public void ExternalAddThreat(int targetID, Vector3 pos, ThreatAddReason reason, float threatIncrementAmount)
        {
            if (_threatList.TryGetValue(targetID, out var target))
            {
                target.IncreaseThreat(threatIncrementAmount);
            }
            else
            {
                Log.Debug("Add threat value fail, do not have this target");
            }
        }

        //TODO 未知id是指玩家id还是Projectile ID
        public void ExternalAddCandidate(int targetID, Vector3 pos, ThreatAddReason reason, float temperatureIncrementAmount)
        {
            //调用AISensingUpdater.CanSignalBeNoticed()方法进行check 是否能加入感知列表
            if (_candidateList.TryGetValue(targetID, out var target))
            {
                target.threatPos = pos;
                target.temperature += temperatureIncrementAmount;
            }
            else
            {
                ThreatInfo info = new ThreatInfo(targetID, pos, reason);
                info.temperature = temperatureIncrementAmount;
                _candidateList.Add(info.id, info);
            }
        }

        /// <summary>
        /// 更新警戒队列
        /// </summary>
        private void UpdateCandidateList()
        {
            //Update candidate from sensible list
            UpdateCandidateFromSensibles();

            foreach (var candidate in _candidateList)
            {
                var addReason = candidate.Value.addReason;
                float distanceToCandidate = Vector3.Distance(candidate.Value.threatPos, knowledge.currentPos);
                float temperatureAmount;
                //if cant see, do not increase temperature
                switch (addReason)
                {
                    case ThreatAddReason.Vision:
                        candidate.Value.lastSeenTime = GameTimerManager.Instance.GetTimeNow();
                        //Get View Attenuation
                        var viewAttenuation = knowledge.threatKnowledge.viewAttenuation;
                        float viewAttenuationAmount = 1f;
                        viewAttenuation.FindY(distanceToCandidate, ref viewAttenuationAmount);

                        //TODO 加上感知随机偏差系数
                        temperatureAmount = knowledge.threatKnowledge.config.feelThreatGrow * Time.deltaTime * viewAttenuationAmount;
                        candidate.Value.IncreaseTemper(temperatureAmount);
                        break;
                    case ThreatAddReason.Feel:
                        candidate.Value.lastFeelTime = Time.deltaTime;
                        //TODO 加上感知随机偏差系数
                        temperatureAmount = knowledge.threatKnowledge.config.feelThreatGrow * Time.deltaTime;
                        candidate.Value.IncreaseTemper(temperatureAmount);
                        break;
                }
            }

            var currentMaxTemperature = 0f;
            for (int i = 0; i < _candidateList.Count; i++)
            {
                var candidate = _candidateList.ElementAt(i).Value;

                currentMaxTemperature = currentMaxTemperature > candidate.temperature
                    ? currentMaxTemperature : candidate.temperature;
            }

            knowledge.temperature = currentMaxTemperature;

            //If knowledge's temperature equals TEMPERVAL_ALERT, do not decrease
            if (knowledge.temperature != ThreatInfo.TEMPERVAL_ALERT)
            {
                float decreaseTemperatureAmount = knowledge.threatKnowledge.config.threatDecreaseSpeed * Time.deltaTime;

                for (int i = 0; i < _candidateList.Count; i++)
                {
                    var candidate = _candidateList.ElementAt(i).Value;

                    candidate.DecreaseTemper(decreaseTemperatureAmount);
                    if (!ValidateCandidate(candidate))
                    {
                        _candidateList.Remove(i);
                    }
                }
            }
        }

        /// <summary>
        /// 从感知模块中更新感知对象的信息
        /// </summary>
        private void UpdateCandidateFromSensibles()
        {
            var sensibles = knowledge.sensingKnowledge.enemySensibles;

            if (sensibles == null)
                return;

            foreach (var sensible in sensibles)
            {
                float distanceToSensible = sensible.Value.distance;

                var feelRange = knowledge.sensingKnowledge.setting.feelRange;
                var viewRange = knowledge.threatLevel == ThreatLevel.Alert ? 200 : knowledge.sensingKnowledge.setting.viewRange;
                var halfHorizontalFov = knowledge.sensingKnowledge.setting.viewPanoramic ? 180f : 0.5 * knowledge.sensingKnowledge.setting.horizontalFov;
                var halfVerticalFov = knowledge.sensingKnowledge.setting.viewPanoramic ? 180f : 0.5 * knowledge.sensingKnowledge.setting.verticalFov;

                if (distanceToSensible <= feelRange)//Feel
                {
                    ProcessSensible(sensible.Value, ThreatAddReason.Feel);
                }
                else if (knowledge.sensingKnowledge.setting.enableVision)//Vision
                {
                    if (distanceToSensible < viewRange)
                    {
                        var horizontalDirection = sensible.Value.direction;
                        horizontalDirection.y = knowledge.eyeTransform.forward.y;
                        var horizontalAngle = Vector3.Angle(knowledge.currentForward, horizontalDirection);
                        if (horizontalAngle < halfHorizontalFov)
                        {
                            var vierticalDirection = sensible.Value.direction;
                            vierticalDirection.x = knowledge.eyeTransform.forward.x;
                            var verticalAngle = Vector3.Angle(knowledge.currentForward, vierticalDirection);
                            if (verticalAngle < halfVerticalFov)
                            {
                                ProcessSensible(sensible.Value, ThreatAddReason.Vision);
                            }
                        }
                    }
                }
            }
        }

        private void UpdateAIKnowledges()
        {
            knowledge.threatKnowledge.reachAwareThisFrame = false;
            knowledge.threatKnowledge.reachAlertThisFrame = false;

            bool threatLevelChange = false;

            //change to ThreatLevel Aware
            if ((knowledge.temperature < ThreatInfo.TEMPERVAL_AWARE) && knowledge.threatLevel != ThreatLevel.Unaware)
            {
                threatLevelChange = true;
                
                knowledge.threatLevel = ThreatLevel.Unaware;
                
            }

            if ((knowledge.temperature > ThreatInfo.TEMPERVAL_AWARE
                && knowledge.temperature < ThreatInfo.TEMPERVAL_ALERT)
                && knowledge.threatLevel != ThreatLevel.Aware)
            {
                threatLevelChange = true;

                knowledge.threatKnowledge.reachAwareThisFrame = true;
                knowledge.threatLevel = ThreatLevel.Aware;
            }

            //change to ThreatLevel Alert
            if (knowledge.temperature >= ThreatInfo.TEMPERVAL_ALERT && knowledge.threatLevel != ThreatLevel.Alert)
            {
                threatLevelChange = true;
                
                knowledge.threatKnowledge.reachAlertThisFrame = true;
                knowledge.threatLevel = ThreatLevel.Alert;
            }

            if (threatLevelChange)
            {
                aiComponent.OnThreatLevelChanged?.Invoke(knowledge.threatLevelOld, knowledge.threatLevel);
                knowledge.aiOwnerEntity?.GetComponent<PoseFSMComponent>()?
                    .SetData(FsmConst.Alertness, (int)knowledge.threatLevel);
            }

            knowledge.threatKnowledge.mainThreat = _mainTarget;
        }

        //TODO 待验证该方法是否如下使用
        private bool ValidateCandidate(ThreatInfo candidate)
        {
            if (candidate.temperature <= 0)
                return false;
            return true;
        }


        /// <summary>
        /// 处理感知目标,根据传入的Threat Add Reason 计算对应的增长
        /// </summary>
        private void ProcessSensible(SensibleInfo sensible, ThreatAddReason sourceType)
        {
            if (!_candidateList.ContainsKey(sensible.sensibleID))
            {
                ThreatInfo threatInfo = new ThreatInfo(sensible.sensibleID, sensible.position, sourceType);
                threatInfo.threatPos = sensible.position;
                //add to candidate list
                _candidateList.TryAdd(threatInfo.id, threatInfo);
            }
            else
            {
                //Renew information
                _candidateList[sensible.sensibleID].threatPos = sensible.position;
                _candidateList[sensible.sensibleID].addReason = sourceType;
            }
        }

        /// <summary>
        /// 更新仇恨列表
        /// </summary>
        private void UpdateThreatList()
        {
            if (knowledge.temperature != ThreatInfo.TEMPERVAL_ALERT)
            {
                if (_threatList.Count > 0)
                    _threatList.Clear();
                return;
            }

            //add all candidate into threat list
            foreach (var candidate in _candidateList)
            {
                if (_threatList.ContainsKey(candidate.Value.id))
                    continue;
                _threatList.Add(candidate.Value.id, candidate.Value);
            }

            //Out of zone
            if (_mainTarget != null)
                if (!ValidateThreat(_mainTarget))
                {
                    _candidateList.Clear();
                    _threatList.Clear();
                    knowledge.temperature = 0;
                }

        }

        private void SelectTarget()
        {
            if (_threatList.Count <= 0)
            {
                _topThreat = null;
                _mainTarget = null;
            }
            else
            {
                bool isThreatTargetChanged = false;

                long oldThreatTargetID = -1;

                _topThreat = _threatList.ElementAt(0).Value;
                for (int i = 0; i < _threatList.Count; i++)
                {
                    if (_threatList.ElementAt(i).Value.threatValue > _topThreat.threatValue)
                    {
                        _topThreat = _threatList.ElementAt(i).Value;
                    }
                }

                if (_mainTarget == null)
                {
                    isThreatTargetChanged = true;
                    oldThreatTargetID = -1;
                    _mainTarget = _topThreat;
                }

                if (_topThreat != null)
                {
                    if (_topThreat.threatValue > 1.2f * _mainTarget.threatValue)
                    {
                        oldThreatTargetID = _mainTarget.id;
                        _mainTarget = _topThreat;
                        isThreatTargetChanged = true;
                    }
                }

                if (isThreatTargetChanged && _mainTarget != null)
                {
                    aiComponent.OnThreatTargetChanged?.Invoke(oldThreatTargetID, _mainTarget.id);
                }
            }
        }

        //TODO
        //Add AIThreatRuntimeInfo
        /// <summary>
        /// 判断是否依旧满足Threat对象
        /// </summary>
        /// <param name="threatInfo"></param>
        /// <returns></returns>
        private bool ValidateThreat(ThreatInfo threatInfo)
        {
            var distanceFromDefendCenter = Vector3.Distance(knowledge.defendAreaKnowledge.defendCenter, threatInfo.threatPos);
            var distanceFromSelf = Vector3.Distance(knowledge.aiOwnerEntity.Position, threatInfo.threatPos);

            //防守区域范围
            var defendRange = knowledge.defendAreaKnowledge.defendRange;
            //边缘距离限制
            var edgeRange = knowledge.threatKnowledge.config.clearThreatEdgeDistance;
            //目标距离限制
            var targetDistance = knowledge.threatKnowledge.config.clearThreatTargetDistance;

            var clearThreatTimerByDistance = knowledge.threatKnowledge.config.clearThreatTimerByDistance;
            var clearThreatTimerByOutOfZone = knowledge.threatKnowledge.config.clearThreatTimerByTargetOutOfZone;

            //超距
            //目标与AI距离 > 目标距离限制
            if (distanceFromSelf > targetDistance)
            {
                if (!threatInfo.lctByFarDistance.IsRunning())
                    threatInfo.lctByFarDistance.Start(GameTimerManager.Instance.GetTimeNow());
                if (threatInfo.lctByFarDistance.IsElapsed(GameTimerManager.Instance.GetTimeNow(), clearThreatTimerByDistance))
                {
                    return false;
                }
            }
            //目标与AI的距离 > (防守区域 + 边缘距离限制)
            else if (distanceFromSelf > (defendRange + edgeRange))
            {
                if (!threatInfo.lctByFarDistance.IsRunning())
                    threatInfo.lctByFarDistance.Start(GameTimerManager.Instance.GetTimeNow());
                if (threatInfo.lctByFarDistance.IsElapsed(GameTimerManager.Instance.GetTimeNow(), clearThreatTimerByDistance))
                {
                    return false;
                }
            }
            else
            {
                threatInfo.lctByFarDistance.Reset(GameTimerManager.Instance.GetTimeNow());
            }

            //超界
            if (distanceFromDefendCenter > defendRange)
            {
                if (!threatInfo.lctByOutOfZone.IsRunning())
                    threatInfo.lctByOutOfZone.Start(GameTimerManager.Instance.GetTimeNow());
                if (threatInfo.lctByOutOfZone.IsElapsed(GameTimerManager.Instance.GetTimeNow(), clearThreatTimerByOutOfZone))
                {
                    return false;
                }
            }
            else
            {
                threatInfo.lctByOutOfZone.Reset(GameTimerManager.Instance.GetTimeNow());
            }
            return true;
        }

        /// <summary>
        /// 移除威胁对象
        /// </summary>
        /// <param name="id"></param>
        private void InternalRemoveThreat(int id)
        {
            if (_threatList.ContainsKey(id))
            {
                if (_mainTarget != null)
                    if (id == _mainTarget.id)
                    {
                        _mainTarget = null;
                    }
                if (_topThreat != null)
                    if (id == _topThreat.id)
                    {
                        _topThreat = null;
                    }
                _threatList.Remove(id);
            }
            else
            {
                Log.Info("Threat List dont contain target id {0}.", id);
            }
        }

        public bool ValidateThreat(ThreatInfo threatInfo, int targetId)
        {
            return true;
        }
        
        //多线程
        //private void CollectThreatListInfoInMainThread(){}
    }
}