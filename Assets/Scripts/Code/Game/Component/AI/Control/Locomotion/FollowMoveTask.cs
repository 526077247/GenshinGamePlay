using UnityEngine;

namespace TaoTie
{
    public class FollowMoveTask: LocoBaseTask
    {
        private Unit anchor; 
        private bool useMeleeSlot; 
        private float stopDistance; 
        private float targetAngle; 
        public override void UpdateLoco(AILocomotionHandler handler, AITransform currentTransform, ref LocoTaskState state)
        {
            Vector3 anchorPos = anchor.Position;

            float distance = Vector3.Distance(currentTransform.pos, anchorPos);
            if (distance > stopDistance)
            {
                var desiredDirection = (anchorPos - currentTransform.pos);
                desiredDirection.y = 0;
                desiredDirection = desiredDirection.normalized;
                handler.aiKnowledge.desiredForward = desiredDirection;

                handler.UpdateMotionFlag(_speedLevel);
            }
        }

        public void Init(AIKnowledge knowledge, AILocomotionHandler.ParamFollowMove param)
        {
            base.Init(knowledge);
            anchor = param.anchor;
            useMeleeSlot = param.useMeleeSlot;
            stopDistance = param.stopDistance;
            targetAngle = param.targetAngle;
            _speedLevel = param.speedLevel;
        }
    }
}