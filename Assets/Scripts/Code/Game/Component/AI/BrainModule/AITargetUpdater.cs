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
        protected override void InitInternal()
        {
            base.InitInternal();
            aiComponent = knowledge.aiOwnerEntity.GetComponent<AIComponent>();
        }


        protected override void UpdateMainThreadInternal()
        {
            Collect(knowledge.targetKnowledge);
            Process(knowledge.targetKnowledge);
        }

        private void Collect(AITargetKnowledge tk)
        {
            bool isSetCombatAttackTarget = false;

            var mainThreat = knowledge.threatKnowledge.mainThreat;

            if (mainThreat != null)
            {
                if (tk.targetEntity == null)
                {
                    tk.SetEntityTarget(AITargetSource.Threat, mainThreat.id, aiComponent);
                    isSetCombatAttackTarget = true;
                }
                else if (tk.targetEntity != null)
                {
                    if (tk.targetEntity.Id != mainThreat.id)
                    {
                        tk.SetEntityTarget(AITargetSource.Threat, mainThreat.id, aiComponent);
                        isSetCombatAttackTarget = true;
                    }
                }
            }

            else
            {
                tk.ClearTarget(AITargetType.EntityTarget);
            }

            if (isSetCombatAttackTarget)
            {
                aiComponent.SetCombatAttackTarget(mainThreat.id);
            }
        }

        private void Process(AITargetKnowledge tk)
        {
            if (tk.targetType== AITargetType.EntityTarget&&tk.targetEntity == null)
                return;
            if (tk.targetType== AITargetType.InvalidTarget)
                return;
            
            var pos = knowledge.aiOwnerEntity.Position;
            Vector3 targetPos = tk.targetPosition;
            if (tk.targetType == AITargetType.EntityTarget)
            {
                targetPos = tk.targetEntity.Position;
            }

            tk.targetDistance = (pos - targetPos).magnitude;
            tk.targetDistanceY = Mathf.Abs(pos.y - targetPos.y);

            pos.y = 0;
            targetPos.y = 0;
            tk.targetDistanceXZ = (pos - targetPos).magnitude;

            var dir = targetPos - knowledge.currentPos;
            if (tk.targetPosition != targetPos && (GameTimerManager.Instance.GetTimeNow() > nextQueryTime ||
                Vector3.SqrMagnitude(tk.targetPosition - targetPos)>1))
            {
                nextQueryTime += 1;
                targetPathQuery?.Dispose();
                targetPathQuery = knowledge.pathFindingKnowledge.CreatePathQueryTask(knowledge.currentPos, targetPos);
            }
            tk.targetPosition = targetPos;
            tk.targetRelativeAngleYaw = Vector3.SignedAngle(knowledge.aiOwnerEntity.Forward, dir, Vector3.up);
            tk.targetRelativeAngleYawAbs = Mathf.Abs(tk.targetRelativeAngleYaw);
            tk.targetRelativeAnglePitch = Vector3.SignedAngle(knowledge.aiOwnerEntity.Forward, dir, Vector3.right);
            tk.targetRelativeAnglePitchAbs = Mathf.Abs(tk.targetRelativeAnglePitch);
            //能否看见
            tk.hasLineOfSight = !PhysicsHelper.LinecastScene(targetPos, knowledge.eyePos, out _);
            //是否在防御范围内
            tk.targetInDefendArea = knowledge.defendAreaKnowledge.CheckInDefendArea(targetPos);
            
            var skillAnchorPosition = knowledge.targetKnowledge.skillAnchorPosition;
            skillAnchorPosition.y = 0;
            tk.skillAnchorDistance = Vector3.Distance(pos, skillAnchorPosition);

            tk.hasPath = AITargetHasPathType.Invalid;
            if (targetPathQuery != null)
            {
                if(targetPathQuery.status == QueryStatus.Success)
                    tk.hasPath = AITargetHasPathType.Success;
                else if(targetPathQuery.status == QueryStatus.Fail)
                    tk.hasPath = AITargetHasPathType.Failed;
            }
        }

        protected override void ClearInternal()
        {
            base.ClearInternal();
            targetPathQuery?.Dispose();
            targetPathQuery = null;
            nextQueryTime = 0;
            aiComponent = null;
        }
    }
}