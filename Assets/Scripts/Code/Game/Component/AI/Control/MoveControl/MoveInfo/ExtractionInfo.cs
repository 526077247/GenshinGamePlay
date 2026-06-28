using UnityEngine;

namespace TaoTie
{
    /// <summary> 撤退：远离目标移动 </summary>
    public class ExtractionInfo : MoveInfoBase
    {
        public enum Status
        {
            Inactive = 0,
            Extracting = 1
        }

        public Status status;
        private const float EXTRACTION_DISTANCE = 10f;

        public static ExtractionInfo Create()
        {
            return ObjectPool.Instance.Fetch<ExtractionInfo>();
        }

        public override void Enter(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge, AIManager aiManager)
        {
            StartExtraction(taskHandler, aiKnowledge);
        }

        public override void UpdateInternal(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge, AIComponent lcai,
            AIManager aiManager)
        {
            if (status == Status.Extracting && taskHandler.CurrentState == LocoTaskState.Finished)
                status = Status.Inactive;
        }

        private void StartExtraction(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge)
        {
            Vector3 awayDir = (aiKnowledge.CurrentPos - aiKnowledge.TargetKnowledge.TargetPosition).normalized;
            awayDir.y = 0;
            Vector3 target = aiKnowledge.CurrentPos + awayDir * EXTRACTION_DISTANCE;
            AILocomotionHandler.ParamGoTo param = new AILocomotionHandler.ParamGoTo
            {
                TargetPosition = target,
                SpeedLevel = MotionFlag.Run
            };
            taskHandler.CreateGoToTask(param);
            status = Status.Extracting;
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
