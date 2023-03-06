using UnityEngine;

namespace TaoTie
{
    public class GoToTask: LocoBaseTask
    {
        private GoToTaskState _innerState;

        private float _getCloseDistance;
        private float _stayCloseOverTime;
        private bool _isInCloseDistance;
        private float _enterCloseDistanceTime;
        private bool _exactlyMove;
        public void Init(AIKnowledge knowledge, AILocomotionHandler.ParamGoTo param)
        {
            base.Init(knowledge);
            speedLevel = param.speedLevel;
            destination = param.targetPosition;
            _getCloseDistance = knowledge.moveKnowledge.GetAlmostReachDistance(param.speedLevel);
            // knowledge.turnSpeed = param.cannedTurnSpeedOverride;
        }
        public override void UpdateLoco(AILocomotionHandler handler, AITransform currentTransform, ref LocoTaskState state)
        {
            var transfomPos = currentTransform.pos;
            transfomPos.y = 0;
            var destination = this.destination;
            destination.y = 0;

            var currentDistance = Vector3.Distance(transfomPos, destination);
            if (currentDistance <= _getCloseDistance)
                stopped = true;
            if (!stopped)
            {
                handler.UpdateMotionFlag(speedLevel);
                var desiredDirection = (destination - transfomPos);
                desiredDirection = desiredDirection.normalized;
                // handler.aiKnowledge.desiredForward = desiredDirection;

                //handler.aiKnowledge.gotoPoint = _destination;
            }
            else
            {
                // if (aiKnowledge.moveKnowledge.onGround)
                //     state = LocoTaskState.Finished;
            }
        }
        public override void RefreshTask(AILocomotionHandler handler, Vector3 positoin)
        {
            stopped = false;
            destination = positoin;
        }
        public override void Deallocate()
        {

        }
    }
}