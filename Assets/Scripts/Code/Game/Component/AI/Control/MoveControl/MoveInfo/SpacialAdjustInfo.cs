using UnityEngine;

namespace TaoTie
{
    /// <summary> 空间调整：调整位置以恢复对目标的视线 </summary>
    public class SpacialAdjustInfo : MoveInfoBase
    {
        public enum Status
        {
            Inactive = 0,
            Adjusting = 1
        }

        public Status status;
        private const float ADJUST_OFFSET = 2f;
        private const int ADJUST_REFRESH_MS = 800;
        private long nextRefreshTick;

        public static SpacialAdjustInfo Create()
        {
            return ObjectPool.Instance.Fetch<SpacialAdjustInfo>();
        }

        public override void Enter(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge, AIManager aiManager)
        {
            StartAdjust(taskHandler, aiKnowledge);
        }

        public override void UpdateInternal(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge, AIComponent lcai,
            AIManager aiManager)
        {
            if (status != Status.Adjusting) return;
            if (aiKnowledge.TargetKnowledge.TargetEntity == null)
            {
                status = Status.Inactive;
                return;
            }
            long timeNow = GameTimerManager.Instance.GetTimeNow();
            if (taskHandler.CurrentState == LocoTaskState.Finished || timeNow >= nextRefreshTick)
            {
                if (aiKnowledge.TargetKnowledge.HasLineOfSight)
                {
                    status = Status.Inactive;
                    return;
                }
                StartAdjust(taskHandler, aiKnowledge);
            }
        }

        private void StartAdjust(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge)
        {
            Vector3 toTarget = aiKnowledge.TargetKnowledge.TargetPosition - aiKnowledge.CurrentPos;
            toTarget.y = 0;
            Vector3 perpendicular = new Vector3(-toTarget.z, 0, toTarget.x).normalized;
            Vector3 destination = aiKnowledge.CurrentPos + perpendicular * ADJUST_OFFSET;
            AILocomotionHandler.ParamGoTo param = new AILocomotionHandler.ParamGoTo
            {
                TargetPosition = destination,
                SpeedLevel = MotionFlag.Walk
            };
            taskHandler.CreateGoToTask(param);
            status = Status.Adjusting;
            nextRefreshTick = GameTimerManager.Instance.GetTimeNow() + ADJUST_REFRESH_MS;
        }

        public override void Leave(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge, AIManager aiManager)
        {
            base.Leave(taskHandler, aiKnowledge, aiManager);
            status = Status.Inactive;
        }

        public override void Dispose()
        {
            status = default;
            nextRefreshTick = 0;
            ObjectPool.Instance.Recycle(this);
        }
    }
}
