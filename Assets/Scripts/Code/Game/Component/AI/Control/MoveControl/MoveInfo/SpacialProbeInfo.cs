using UnityEngine;

namespace TaoTie
{
    /// <summary> 空间探查：向目标高度移动以探查空中 </summary>
    public class SpacialProbeInfo : MoveInfoBase
    {
        public enum Status
        {
            Inactive = 0,
            Probing = 1
        }

        public Status status;
        private const float PROBE_TARGET_Y_OFFSET = 2f;

        public static SpacialProbeInfo Create()
        {
            return ObjectPool.Instance.Fetch<SpacialProbeInfo>();
        }

        public override void Enter(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge, AIManager aiManager)
        {
            StartProbe(taskHandler, aiKnowledge);
        }

        public override void UpdateInternal(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge, AIComponent lcai,
            AIManager aiManager)
        {
            if (status != Status.Probing) return;
            if (taskHandler.CurrentState == LocoTaskState.Finished)
                status = Status.Inactive;
        }

        private void StartProbe(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge)
        {
            Unit target = aiKnowledge.TargetKnowledge.TargetEntity;
            if (target == null)
            {
                status = Status.Inactive;
                return;
            }
            Vector3 targetPos = target.Position + Vector3.up * PROBE_TARGET_Y_OFFSET;
            AILocomotionHandler.ParamGoTo param = new AILocomotionHandler.ParamGoTo
            {
                TargetPosition = targetPos,
                SpeedLevel = MotionFlag.Run
            };
            taskHandler.CreateGoToTask(param);
            status = Status.Probing;
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
