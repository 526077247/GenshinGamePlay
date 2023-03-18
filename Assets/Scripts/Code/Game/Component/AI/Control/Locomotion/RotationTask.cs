using UnityEngine;

namespace TaoTie
{
    public class RotationTask: LocoBaseTask
    {
        private Vector3 targetPosition;
        private static readonly int TIMEOUT = 1000;
        private long timeoutTick;
        public void Init(AIKnowledge knowledge, AILocomotionHandler.ParamRotation param)
        {
            base.Init(knowledge);
            targetPosition = param.targetPosition;
            timeoutTick = GameTimerManager.Instance.GetTimeNow() + TIMEOUT;
            Messager.Instance.Broadcast(aiKnowledge.aiOwnerEntity.Id, MessageId.UpdateTurnTargetPos, targetPosition,
                TIMEOUT);
        }
        public override void UpdateLoco(AILocomotionHandler handler, AITransform currentTransform, ref LocoTaskState state)
        {
            if (GameTimerManager.Instance.GetTimeNow() >= timeoutTick)
            {
                state = LocoTaskState.Finished;
            }
        }

        public override void OnCloseTask(AILocomotionHandler handler)
        {
            
        }
        public override void Deallocate()
        {
            
        }
    }
}