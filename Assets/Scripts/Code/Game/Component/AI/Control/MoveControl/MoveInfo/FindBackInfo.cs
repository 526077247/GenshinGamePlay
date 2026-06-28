using UnityEngine;

namespace TaoTie
{
    /// <summary> 绕背：向侧后方移动以绕到目标背后 </summary>
    public class FindBackInfo : MoveInfoBase
    {
        public enum Status
        {
            Inactive = 0,
            Flanking = 1
        }

        public Status status;
        private const float FLANK_DURATION_MS = 2000;

        public static FindBackInfo Create()
        {
            return ObjectPool.Instance.Fetch<FindBackInfo>();
        }

        public override void Enter(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge, AIManager aiManager)
        {
            StartFlank(taskHandler, aiKnowledge);
        }

        public override void UpdateInternal(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge, AIComponent lcai,
            AIManager aiManager)
        {
            if (status != Status.Flanking) return;
            if (taskHandler.CurrentState == LocoTaskState.Finished)
                status = Status.Inactive;
        }

        private void StartFlank(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge)
        {
            Unit target = aiKnowledge.TargetKnowledge.TargetEntity;
            if (target == null)
            {
                status = Status.Inactive;
                return;
            }
            MotionDirection dir = Random.value > 0.5f ? MotionDirection.Left : MotionDirection.Right;
            AILocomotionHandler.ParamFacingMove param = new AILocomotionHandler.ParamFacingMove
            {
                Anchor = target,
                SpeedLevel = MotionFlag.Run,
                Duration = FLANK_DURATION_MS,
                MovingDirection = dir
            };
            taskHandler.CreateFacingMoveTask(param);
            status = Status.Flanking;
        }

        public override void Leave(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge, AIManager aiManager)
        {
            base.Leave(taskHandler, aiKnowledge, aiManager);
            status = Status.Inactive;
        }

        public override void Dispose()
        {
            status = default;
            ObjectPool.Instance.Recycle(this);
        }
    }
}
