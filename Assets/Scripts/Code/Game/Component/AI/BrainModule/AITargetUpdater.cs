using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 目标
    /// </summary>
    public class AITargetUpdater: BrainModuleBase
    {
        private AIComponent aiComponent;
        private PathQueryTask targetPathQuery;
        private long nextQueryTime;
        /// <summary>
        /// 上一次寻路目的点
        /// </summary>
        private Vector3 lastQueryTarget;
        /// <summary>
        /// 上一次处理的目标ID
        /// </summary>
        private long lastTargetID;

        /// <summary>
        /// 当前目标寻路任务
        /// </summary>
        public PathQueryTask TargetPathQuery => targetPathQuery;
        protected override void InitInternal()
        {
            base.InitInternal();
            aiComponent = knowledge.Entity.GetComponent<AIComponent>();
        }


        protected override void UpdateMainThreadInternal()
        {
            Collect(knowledge.TargetKnowledge);
            Process(knowledge.TargetKnowledge);
        }

        private void Collect(AITargetKnowledge tk)
        {
            bool isSetCombatAttackTarget = false;

            var mainThreat = knowledge.ThreatKnowledge.MainThreat;

            if (mainThreat != null)
            {
                if (tk.TargetEntity == null)
                {
                    tk.SetEntityTarget(AITargetSource.Threat, mainThreat.Id, aiComponent);
                    isSetCombatAttackTarget = true;
                }
                else
                {
                    if (tk.TargetEntity.Id != mainThreat.Id)
                    {
                        tk.SetEntityTarget(AITargetSource.Threat, mainThreat.Id, aiComponent);
                        isSetCombatAttackTarget = true;
                    }

                    if (tk.TargetEntity.GetComponent<CombatComponent>() == null)
                    {
                        mainThreat.DecreaseThreat(ThreatInfo.THREATVAL_MAX);
                    }
                }
                            
                if (knowledge.ThreatKnowledge.Config.ClearThreatByLostPath)
                {
                    var timeNow = GameTimerManager.Instance.GetTimeNow();
                    if (tk.HasPath == AITargetHasPathType.Failed)
                        mainThreat.LctByEntityDisappear.Start(timeNow);
                    if (mainThreat.LctByFarDistance.IsElapsed(timeNow, knowledge.ThreatKnowledge.Config.ClearThreatTimerByLostPath))
                    {
                        mainThreat.DecreaseThreat(ThreatInfo.THREATVAL_MAX);
                    }
                }
            }

            else
            {
                tk.ClearTarget(AITargetType.EntityTarget);
            }

            if (isSetCombatAttackTarget)
            {
                aiComponent.SetCombatAttackTarget(mainThreat.Id);
            }
        }

        private void Process(AITargetKnowledge tk)
        {
            tk.HasLineOfSight = false;
            tk.HasPath = AITargetHasPathType.Invalid;
            if (tk.TargetType== AITargetType.EntityTarget&&tk.TargetEntity == null)
                return;
            if (tk.TargetType== AITargetType.InvalidTarget)
                return;
            
            if (tk.TargetType!= AITargetType.InvalidTarget && tk.TargetID != lastTargetID)
            {
                lastTargetID = tk.TargetID;
                lastQueryTarget = Vector3.zero;
                nextQueryTime = 0;
            }
            var pos = knowledge.Entity.Position;
            Vector3 targetPos = tk.TargetPosition;
            if (tk.TargetType == AITargetType.EntityTarget)
            {
                targetPos = tk.TargetEntity.Position;
            }

            //能否看见
            if (knowledge.SensingKnowledge.EnemySensibles?.TryGetValue(tk.TargetID, out var sensible) == true)
            {
                tk.HasLineOfSight = sensible.HasLineOfSight;
                //能看见时刷新最后感知位置
                if (tk.HasLineOfSight)
                {
                    tk.TargetLKP = targetPos;
                }
            }
            //统一以最后感知位置(LKP)作为寻路/移动目的地；没有LKP即感知不到目标位置，不进行追踪
            var pathTarget = tk.TargetLKP;

            tk.TargetDistance = (pos - targetPos).magnitude;
            tk.TargetDistanceY = pos.y - targetPos.y;

            var posXZ = pos;
            posXZ.y = 0;
            var targetPosXZ = targetPos;
            targetPosXZ.y = 0;
            tk.TargetDistanceXZ = (posXZ - targetPosXZ).magnitude;

            var dir = targetPosXZ - knowledge.CurrentPos;
            if (pathTarget != null)
            {
                var pathPos = pathTarget.Value;
                var delta = pathPos - lastQueryTarget;
                var movedXZ = delta.x * delta.x + delta.z * delta.z > 1f;
                var movedY = Mathf.Abs(delta.y) > 2f;
                if (lastQueryTarget != pathPos && (GameTimerManager.Instance.GetTimeNow() > nextQueryTime ||
                    movedXZ || movedY))
                {
                    nextQueryTime += 1000;
                    targetPathQuery?.Dispose();
                    targetPathQuery = knowledge.PathFindingKnowledge.CreatePathQueryTask(knowledge.CurrentPos, pathPos);
                    lastQueryTarget = pathPos;
                }
            }
            else
            {
                //感知不到：无可追踪位置，丢弃已有寻路结果
                targetPathQuery?.Dispose();
                targetPathQuery = null;
                lastQueryTarget = Vector3.zero;
            }
            tk.TargetPosition = targetPos;
            tk.TargetRelativeAngleYaw = Vector3.SignedAngle(knowledge.Entity.Forward, dir, Vector3.up);
            tk.TargetRelativeAngleYawAbs = Mathf.Abs(tk.TargetRelativeAngleYaw);
            tk.TargetRelativeAnglePitch = Vector3.SignedAngle(knowledge.Entity.Forward, dir, Vector3.right);
            tk.TargetRelativeAnglePitchAbs = Mathf.Abs(tk.TargetRelativeAnglePitch);
            //是否在防御范围内
            tk.TargetInDefendArea = knowledge.DefendAreaKnowledge.CheckInDefendArea(targetPos);
            
            var skillAnchorPosition = knowledge.TargetKnowledge.SkillAnchorPosition;
            skillAnchorPosition.y = 0;
            tk.SkillAnchorDistance = Vector3.Distance(posXZ, skillAnchorPosition);
            
            if (targetPathQuery != null)
            {
                if (targetPathQuery.Status == QueryStatus.Success)
                {
                    //直线寻路：仅当自身与寻路目的地之间无障碍时才算可寻路
                    if ((targetPathQuery.Type == NavMeshUseType.NotUse ||
                         (targetPathQuery.Type == NavMeshUseType.Auto &&
                          knowledge.PathFindingKnowledge.Type == PathFindingType.Link)) &&
                        PhysicsHelper.LinecastScene(knowledge.CurrentPos, targetPathQuery.Destination, out _))
                    {
                        tk.HasPath = AITargetHasPathType.Failed;
                    }
                    else
                    {
                        tk.HasPath = AITargetHasPathType.Success;
                    }
                }
                else if (targetPathQuery.Status == QueryStatus.Fail)
                {
                    tk.HasPath = AITargetHasPathType.Failed;
                }
            }
        }

        protected override void ClearInternal()
        {
            base.ClearInternal();
            targetPathQuery?.Dispose();
            targetPathQuery = null;
            nextQueryTime = 0;
            lastQueryTarget = Vector3.zero;
            lastTargetID = 0;
            aiComponent = null;
        }
    }
}