using UnityEngine;

namespace TaoTie
{
    /// <summary> 前往最后已知位置调查 </summary>
    public class InvestigateInfo : MoveInfoBase
    {
        public enum Status
        {
            Inactive = 0,
            Investigating = 1
        }

        public Status status;

        public static InvestigateInfo Create()
        {
            return ObjectPool.Instance.Fetch<InvestigateInfo>();
        }

        public override void Enter(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge, AIManager aiManager)
        {
            StartInvestigate(taskHandler, aiKnowledge);
        }

        public override void UpdateInternal(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge, AIComponent lcai,
            AIManager aiManager)
        {
            if (status == Status.Investigating && taskHandler.CurrentState == LocoTaskState.Finished)
                status = Status.Inactive;
        }

        private void StartInvestigate(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge)
        {
            Vector3? lkp = aiKnowledge.TargetKnowledge.TargetLKP;
            if (lkp == null)
            {
                status = Status.Inactive;
                return;
            }
            AILocomotionHandler.ParamGoTo param = new AILocomotionHandler.ParamGoTo
            {
                TargetPosition = (Vector3)lkp,
                SpeedLevel = MotionFlag.Run
            };
            taskHandler.CreateGoToTask(param);
            status = Status.Investigating;
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
