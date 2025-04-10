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
                Stopped = true;
                handler.UpdateMotionFlag(MotionFlag.Idle);
                state = LocoTaskState.Finished;
            }
            else
            {
                destination = anchor.Position;
            }
            if (!Stopped)
            {
                float distance = Vector3.Distance(currentTransform.Position, destination);
                if (distance > stopDistance)
                {
                    handler.UpdateMotionFlag(speedLevel);
                    handler.ForceLookAt();
                }
                else
                {
                    Stopped = true;
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
            anchor = param.Anchor;
            useMeleeSlot = param.UseMeleeSlot;
            stopDistance = param.StopDistance;
            targetAngle = param.TargetAngle;
            speedLevel = param.SpeedLevel;
        }
    }
}