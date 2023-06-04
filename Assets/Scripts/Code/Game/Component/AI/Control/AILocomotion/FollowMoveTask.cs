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
            if (anchor == null)
            {
                stopped = true;
                handler.UpdateMotionFlag(MotionFlag.Idle);
                state = LocoTaskState.Finished;
            }
            destination = anchor.Position;
            if (!stopped)
            {
                float distance = Vector3.Distance(currentTransform.pos, destination);
                if (distance > stopDistance)
                {
                    handler.UpdateMotionFlag(speedLevel);
                    handler.ForceLookAt();
                }
                else
                {
                    stopped = true;
                    handler.UpdateMotionFlag(MotionFlag.Idle);
                    state = LocoTaskState.Finished;
                }
            }
            else
            {
                handler.UpdateMotionFlag(MotionFlag.Idle);
                state = LocoTaskState.Finished;
            }
        }

        public void Init(AIKnowledge knowledge, AILocomotionHandler.ParamFollowMove param)
        {
            base.Init(knowledge);
            anchor = param.anchor;
            useMeleeSlot = param.useMeleeSlot;
            stopDistance = param.stopDistance;
            targetAngle = param.targetAngle;
            speedLevel = param.speedLevel;
        }
    }
}