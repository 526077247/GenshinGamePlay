using UnityEngine;

namespace TaoTie
{
    public class FacingMoveTask: LocoBaseTask
    {
        private enum FacingMoveTaskState
        {
            Start = 0,
            Moving = 1
        }
        private FacingMoveTaskState innerState;
        private Unit anchor;
        private MotionDirection movingDirection;
        private float duration;
        private float finishTick;
        public override void UpdateLoco(AILocomotionHandler handler, AITransform currentTransform, ref LocoTaskState state)
        {
            if(anchor!=null)
                destination = anchor.Position;
            if (innerState == FacingMoveTaskState.Start)
            {
                handler.UpdateMotionFlag(speedLevel, movingDirection);
                finishTick = GameTimerManager.Instance.GetTimeNow() + duration;
                innerState = FacingMoveTaskState.Moving;
            }

            if (innerState == FacingMoveTaskState.Moving)
            {
                if (GameTimerManager.Instance.GetTimeNow() >= finishTick)
                {
                    innerState = FacingMoveTaskState.Start;
                }
                handler.ForceLookAt();
            }

            if (Stopped)
            {
                state = LocoTaskState.Finished;
                handler.UpdateMotionFlag(0);
            }
        }
        
        public void Init(AIKnowledge knowledge, AILocomotionHandler.ParamFacingMove param)
        {
            base.Init(knowledge);
            anchor = param.anchor;
            speedLevel = param.speedLevel;
            movingDirection = param.movingDirection;
            if(anchor!=null)
                destination = anchor.Position;
            innerState = FacingMoveTaskState.Start;
            duration = param.duration;
            
        }
    }
}