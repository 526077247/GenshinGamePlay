using UnityEngine;

namespace TaoTie
{
    public class RotationTask: LocoBaseTask
    {
        private const int TIMEOUT = 1000;
        
        private Vector3 targetPosition;
        private long timeoutTick;
        public void Init(AIKnowledge knowledge, AILocomotionHandler.ParamRotation param)
        {
            base.Init(knowledge);
            targetPosition = param.TargetPosition;
            timeoutTick = GameTimerManager.Instance.GetTimeNow() + TIMEOUT;
            Messager.Instance.Broadcast(base.knowledge.Entity.Id, MessageId.UpdateTurnTargetPos, targetPosition,
                TIMEOUT);
        }
        public override void UpdateLoco(AILocomotionHandler handler, AITransform currentTransform, ref LocoTaskState state)
        {
            if (GameTimerManager.Instance.GetTimeNow() >= timeoutTick)
            {
                state = LocoTaskState.Finished;
            }
        }
    }
}