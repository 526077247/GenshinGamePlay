using UnityEngine;

namespace TaoTie
{
    /// <summary> 环绕对峙：围绕目标做环绕移动 </summary>
    public class SurroundInfo : MoveInfoBase
    {
        public enum Status
        {
            Inactive = 0,
            Surrounding = 1
        }

        public Status status;
        private const float SURROUND_RADIUS = 4f;
        private const int SURROUND_DURATION_MS = 3000;
        private long finishTick;

        public static SurroundInfo Create()
        {
            return ObjectPool.Instance.Fetch<SurroundInfo>();
        }

        public override void Enter(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge, AIManager aiManager)
        {
            StartSurround(taskHandler, aiKnowledge);
        }

        public override void UpdateInternal(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge, AIComponent lcai,
            AIManager aiManager)
        {
            if (status != Status.Surrounding) return;
            if (aiKnowledge.TargetKnowledge.TargetEntity == null)
            {
                status = Status.Inactive;
                return;
            }
            if (taskHandler.CurrentState == LocoTaskState.Finished || GameTimerManager.Instance.GetTimeNow() >= finishTick)
            {
                StartSurround(taskHandler, aiKnowledge);
            }
        }

        private void StartSurround(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge)
        {
            Unit target = aiKnowledge.TargetKnowledge.TargetEntity;
            if (target == null)
            {
                status = Status.Inactive;
                return;
            }
            bool clockwise = Random.value > 0.5f;
            AILocomotionHandler.ParamSurroundDash param = new AILocomotionHandler.ParamSurroundDash
            {
                Anchor = target,
                SpeedLevel = MotionFlag.Walk,
                Clockwise = clockwise,
                Radius = SURROUND_RADIUS,
                DelayStopping = false
            };
            taskHandler.CreateSurroundDashTask(param);
            status = Status.Surrounding;
            finishTick = GameTimerManager.Instance.GetTimeNow() + SURROUND_DURATION_MS;
        }

        public override void Leave(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge, AIManager aiManager)
        {
            base.Leave(taskHandler, aiKnowledge, aiManager);
            status = Status.Inactive;
        }

        public override void Dispose()
        {
            status = default;
            finishTick = 0;
            ObjectPool.Instance.Recycle(this);
        }
    }
}
