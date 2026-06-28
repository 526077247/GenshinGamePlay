using UnityEngine;

namespace TaoTie
{
    /// <summary> 空间追击：追击空中目标 </summary>
    public class SpacialChaseInfo : MoveInfoBase
    {
        public enum Status
        {
            Inactive = 0,
            Chasing = 1
        }

        public Status status;
        private const int CHASE_REFRESH_INTERVAL_MS = 500;
        private long nextRefreshTick;

        public static SpacialChaseInfo Create()
        {
            return ObjectPool.Instance.Fetch<SpacialChaseInfo>();
        }

        public override void Enter(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge, AIManager aiManager)
        {
            StartChase(taskHandler, aiKnowledge);
        }

        public override void UpdateInternal(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge, AIComponent lcai,
            AIManager aiManager)
        {
            if (status != Status.Chasing) return;
            if (aiKnowledge.TargetKnowledge.TargetEntity == null)
            {
                status = Status.Inactive;
                return;
            }
            long timeNow = GameTimerManager.Instance.GetTimeNow();
            if (taskHandler.CurrentState == LocoTaskState.Finished || timeNow >= nextRefreshTick)
            {
                StartChase(taskHandler, aiKnowledge);
            }
        }

        private void StartChase(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge)
        {
            Unit target = aiKnowledge.TargetKnowledge.TargetEntity;
            if (target == null)
            {
                status = Status.Inactive;
                return;
            }
            AILocomotionHandler.ParamGoTo param = new AILocomotionHandler.ParamGoTo
            {
                TargetPosition = target.Position,
                SpeedLevel = MotionFlag.Run
            };
            taskHandler.CreateGoToTask(param);
            status = Status.Chasing;
            nextRefreshTick = GameTimerManager.Instance.GetTimeNow() + CHASE_REFRESH_INTERVAL_MS;
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
