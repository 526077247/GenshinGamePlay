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
            aiComponent = knowledge.AiOwnerEntity.GetComponent<AIComponent>();
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
                else if (tk.TargetEntity != null)
                {
                    if (tk.TargetEntity.Id != mainThreat.Id)
                    {
                        tk.SetEntityTarget(AITargetSource.Threat, mainThreat.Id, aiComponent);
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
                aiComponent.SetCombatAttackTarget(mainThreat.Id);
            }
        }

        private void Process(AITargetKnowledge tk)
        {
            if (tk.TargetType== AITargetType.EntityTarget&&tk.TargetEntity == null)
                return;
            if (tk.TargetType== AITargetType.InvalidTarget)
                return;
            
            var pos = knowledge.AiOwnerEntity.Position;
            Vector3 targetPos = tk.TargetPosition;
            if (tk.TargetType == AITargetType.EntityTarget)
            {
                targetPos = tk.TargetEntity.Position;
            }

            tk.TargetDistance = (pos - targetPos).magnitude;
            tk.TargetDistanceY = Mathf.Abs(pos.y - targetPos.y);

            pos.y = 0;
            targetPos.y = 0;
            tk.TargetDistanceXZ = (pos - targetPos).magnitude;

            var dir = targetPos - knowledge.CurrentPos;
            if (tk.TargetPosition != targetPos && (GameTimerManager.Instance.GetTimeNow() > nextQueryTime ||
                Vector3.SqrMagnitude(tk.TargetPosition - targetPos)>1))
            {
                nextQueryTime += 1;
                targetPathQuery?.Dispose();
                targetPathQuery = knowledge.PathFindingKnowledge.CreatePathQueryTask(knowledge.CurrentPos, targetPos);
            }
            tk.TargetPosition = targetPos;
            tk.TargetRelativeAngleYaw = Vector3.SignedAngle(knowledge.AiOwnerEntity.Forward, dir, Vector3.up);
            tk.TargetRelativeAngleYawAbs = Mathf.Abs(tk.TargetRelativeAngleYaw);
            tk.TargetRelativeAnglePitch = Vector3.SignedAngle(knowledge.AiOwnerEntity.Forward, dir, Vector3.right);
            tk.TargetRelativeAnglePitchAbs = Mathf.Abs(tk.TargetRelativeAnglePitch);
            //能否看见
            tk.HasLineOfSight = !PhysicsHelper.LinecastScene(targetPos, knowledge.EyePos, out _);
            //是否在防御范围内
            tk.TargetInDefendArea = knowledge.DefendAreaKnowledge.CheckInDefendArea(targetPos);
            
            var skillAnchorPosition = knowledge.TargetKnowledge.SkillAnchorPosition;
            skillAnchorPosition.y = 0;
            tk.SkillAnchorDistance = Vector3.Distance(pos, skillAnchorPosition);

            tk.HasPath = AITargetHasPathType.Invalid;
            if (targetPathQuery != null)
            {
                if(targetPathQuery.Status == QueryStatus.Success)
                    tk.HasPath = AITargetHasPathType.Success;
                else if(targetPathQuery.Status == QueryStatus.Fail)
                    tk.HasPath = AITargetHasPathType.Failed;
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