using UnityEngine;

namespace TaoTie
{
    /// <summary> 降落：从空中下降到地面高度 </summary>
    public class LandingInfo : MoveInfoBase
    {
        public enum Status
        {
            Inactive = 0,
            Landing = 1
        }

        public Status status;
        private const float GROUND_Y_OFFSET = 0f;

        public static LandingInfo Create()
        {
            return ObjectPool.Instance.Fetch<LandingInfo>();
        }

        public override void Enter(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge, AIManager aiManager)
        {
            StartLanding(taskHandler, aiKnowledge);
        }

        public override void UpdateInternal(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge, AIComponent lcai,
            AIManager aiManager)
        {
            if (status == Status.Landing)
            {
                if (aiKnowledge.CurrentPos.y <= aiKnowledge.BornPos.y + GROUND_Y_OFFSET + 0.5f)
                    status = Status.Inactive;
            }
        }

        private void StartLanding(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge)
        {
            Vector3 target = new Vector3(
                aiKnowledge.CurrentPos.x,
                aiKnowledge.BornPos.y + GROUND_Y_OFFSET,
                aiKnowledge.CurrentPos.z
            );
            AILocomotionHandler.ParamGoTo param = new AILocomotionHandler.ParamGoTo
            {
                TargetPosition = target,
                SpeedLevel = MotionFlag.Walk
            };
            taskHandler.CreateGoToTask(param);
            status = Status.Landing;
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
