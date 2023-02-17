using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 目标
    /// </summary>
    public class AITargetUpdater: BrainModuleBase
    {
        private AIComponent aiComponent;
        public AITargetUpdater(AIComponent aiComponent)
        {
            this.aiComponent = aiComponent;
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
            if (tk.targetEntity == null)
                return;
            
            var pos = knowledge.aiOwnerEntity.Position;
            var targetPos = tk.targetEntity.Position;

            tk.targetDistance = (pos - targetPos).magnitude;
            tk.targetDistanceY = Mathf.Abs(pos.y - targetPos.y);

            pos.y = 0;
            targetPos.y = 0;
            tk.targetDistanceXZ = (pos - targetPos).magnitude;

            var dir = tk.targetEntity.Position - knowledge.aiOwnerEntity.Position;
            tk.targetPosition = targetPos;
            tk.targetRelativeAngleYaw = Vector3.SignedAngle(knowledge.aiOwnerEntity.Forward, dir, Vector3.up);
            tk.targetRelativeAngleYawAbs = Mathf.Abs(tk.targetRelativeAngleYaw);
            tk.targetRelativeAnglePitch = Vector3.SignedAngle(knowledge.aiOwnerEntity.Forward, dir, Vector3.right);
            tk.targetRelativeAnglePitchAbs = Mathf.Abs(tk.targetRelativeAnglePitch);
            //能否看见
            tk.hasLineOfSight = !PhysicsHelper.LinecastScene(tk.targetEntity.Position, knowledge.eyePos);
            
            var skillAnchorPosition = knowledge.targetKnowledge.skillAnchorPosition;
            skillAnchorPosition.y = 0;
            tk.skillAnchorDistance = Vector3.Distance(pos, skillAnchorPosition);
        }
    }
}