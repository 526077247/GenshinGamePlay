using UnityEngine;

namespace TaoTie
{
    public class GoToTask: LocoBaseTask
    {
        private GoToTaskState _innerState;

        private float getCloseDistance;
        private float turnSpeed;
        
        public void Init(AIKnowledge knowledge, AILocomotionHandler.ParamGoTo param)
        {
            base.Init(knowledge);
            speedLevel = param.speedLevel;
            destination = param.targetPosition;
            getCloseDistance = knowledge.MoveKnowledge.GetAlmostReachDistance(param.speedLevel);
            turnSpeed = param.cannedTurnSpeedOverride;
        }
        public override void UpdateLoco(AILocomotionHandler handler, AITransform currentTransform, ref LocoTaskState state)
        {
            var transfomPos = currentTransform.pos;
            transfomPos.y = 0;
            destination.y = 0;
            
            if (!stopped)
            {
                var currentDistance = Vector3.Distance(transfomPos, destination);
                if (currentDistance <= getCloseDistance)
                {
                    stopped = true;
                    state = LocoTaskState.Finished;
                    handler.UpdateMotionFlag(0);
                }
                else
                {
                    handler.UpdateMotionFlag(speedLevel);
                    if (turnSpeed != 0)
                    {
                        handler.UpdateTurnSpeed(turnSpeed);
                    }
                }
            }
            else
            {
                state = LocoTaskState.Finished;
                handler.UpdateMotionFlag(0);
            }
        }
        public override void RefreshTask(AILocomotionHandler handler, Vector3 positoin)
        {
            stopped = false;
            destination = positoin;
        }
    }
}