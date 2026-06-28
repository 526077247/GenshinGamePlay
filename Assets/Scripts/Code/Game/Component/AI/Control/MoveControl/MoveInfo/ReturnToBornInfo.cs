using UnityEngine;

namespace TaoTie
{
    /// <summary> 返回出生点 </summary>
    public class ReturnToBornInfo : MoveInfoBase
    {
        public enum Status
        {
            Inactive = 0,
            Returning = 1
        }

        public Status status;

        public static ReturnToBornInfo Create()
        {
            return ObjectPool.Instance.Fetch<ReturnToBornInfo>();
        }

        public override void Enter(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge, AIManager aiManager)
        {
            StartReturn(taskHandler, aiKnowledge);
        }

        public override void UpdateInternal(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge, AIComponent lcai,
            AIManager aiManager)
        {
            if (status == Status.Returning && taskHandler.CurrentState == LocoTaskState.Finished)
            {
                float dist = Vector3.Distance(aiKnowledge.CurrentPos, aiKnowledge.BornPos);
                if (dist < 0.5f)
                    status = Status.Inactive;
                else
                    StartReturn(taskHandler, aiKnowledge);
            }
        }

        private void StartReturn(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge)
        {
            AILocomotionHandler.ParamGoTo param = new AILocomotionHandler.ParamGoTo
            {
                TargetPosition = aiKnowledge.BornPos,
                SpeedLevel = MotionFlag.Run
            };
            taskHandler.CreateGoToTask(param);
            status = Status.Returning;
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
