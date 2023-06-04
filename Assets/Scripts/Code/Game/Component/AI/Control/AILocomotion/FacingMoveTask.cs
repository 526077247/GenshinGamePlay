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
        
        private Unit anchor;
        private MotionDirection movingDirection;

        public override void UpdateLoco(AILocomotionHandler handler, AITransform currentTransform, ref LocoTaskState state)
        {
            // handler.UpdateMotionFlag(movingDirection,speedLevel);
        }
        
        public void Init(AIKnowledge knowledge, AILocomotionHandler.ParamFacingMove param)
        {
            base.Init(knowledge);
            anchor = param.anchor;
            speedLevel = param.speedLevel;
            movingDirection = param.movingDirection;
            destination = anchor.Position;
        }
    }
}