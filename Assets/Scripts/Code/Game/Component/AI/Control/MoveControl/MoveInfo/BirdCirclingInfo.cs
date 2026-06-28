using UnityEngine;

namespace TaoTie
{
    /// <summary> 飞鸟盘旋：围绕目标做大半径空中盘旋 </summary>
    public class BirdCirclingInfo : MoveInfoBase
    {
        public enum Status
        {
            Inactive = 0,
            Circling = 1
        }

        public Status status;
        private const float CIRCLE_RADIUS = 8f;
        private const int CIRCLE_DURATION_MS = 5000;
        private long finishTick;

        public static BirdCirclingInfo Create()
        {
            return ObjectPool.Instance.Fetch<BirdCirclingInfo>();
        }

        public override void Enter(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge, AIManager aiManager)
        {
            StartCircling(taskHandler, aiKnowledge);
        }

        public override void UpdateInternal(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge, AIComponent lcai,
            AIManager aiManager)
        {
            if (status != Status.Circling) return;
            if (aiKnowledge.TargetKnowledge.TargetEntity == null)
            {
                status = Status.Inactive;
                return;
            }
            if (taskHandler.CurrentState == LocoTaskState.Finished || GameTimerManager.Instance.GetTimeNow() >= finishTick)
            {
                StartCircling(taskHandler, aiKnowledge);
            }
        }

        private void StartCircling(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge)
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
                SpeedLevel = MotionFlag.Run,
                Clockwise = clockwise,
                Radius = CIRCLE_RADIUS,
                DelayStopping = false
            };
            taskHandler.CreateSurroundDashTask(param);
            status = Status.Circling;
            finishTick = GameTimerManager.Instance.GetTimeNow() + CIRCLE_DURATION_MS;
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
