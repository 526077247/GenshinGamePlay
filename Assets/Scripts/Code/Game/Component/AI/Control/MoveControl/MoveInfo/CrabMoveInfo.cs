using UnityEngine;

namespace TaoTie
{
    /// <summary> 螃蟹步：近距离左右横移闪避 </summary>
    public class CrabMoveInfo : MoveInfoBase
    {
        public enum Status
        {
            Inactive = 0,
            Strafing = 1
        }

        public Status status;
        private const float STRAFE_DURATION_MS = 1500;

        public static CrabMoveInfo Create()
        {
            return ObjectPool.Instance.Fetch<CrabMoveInfo>();
        }

        public override void Enter(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge, AIManager aiManager)
        {
            StartStrafe(taskHandler, aiKnowledge);
        }

        public override void UpdateInternal(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge, AIComponent lcai,
            AIManager aiManager)
        {
            if (status != Status.Strafing) return;
            if (taskHandler.CurrentState == LocoTaskState.Finished)
                StartStrafe(taskHandler, aiKnowledge);
        }

        private void StartStrafe(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge)
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
                SpeedLevel = MotionFlag.Walk,
                Duration = STRAFE_DURATION_MS,
                MovingDirection = dir
            };
            taskHandler.CreateFacingMoveTask(param);
            status = Status.Strafing;
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
