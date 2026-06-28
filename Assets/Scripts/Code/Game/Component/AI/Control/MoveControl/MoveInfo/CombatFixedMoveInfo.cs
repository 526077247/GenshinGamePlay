using UnityEngine;

namespace TaoTie
{
    /// <summary> 战斗固定移动：朝目标方向固定路线移动 </summary>
    public class CombatFixedMoveInfo : MoveInfoBase
    {
        public enum Status
        {
            Inactive = 0,
            Moving = 1
        }

        public Status status;
        private const float FIXED_MOVE_DISTANCE = 3f;

        public static CombatFixedMoveInfo Create()
        {
            return ObjectPool.Instance.Fetch<CombatFixedMoveInfo>();
        }

        public override void Enter(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge, AIManager aiManager)
        {
            StartMove(taskHandler, aiKnowledge);
        }

        public override void UpdateInternal(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge, AIComponent lcai,
            AIManager aiManager)
        {
            if (status != Status.Moving) return;
            if (taskHandler.CurrentState == LocoTaskState.Finished)
                status = Status.Inactive;
        }

        private void StartMove(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge)
        {
            Unit target = aiKnowledge.TargetKnowledge.TargetEntity;
            if (target == null)
            {
                status = Status.Inactive;
                return;
            }
            Vector3 dir = (target.Position - aiKnowledge.CurrentPos).normalized;
            dir.y = 0;
            Vector3 destination = aiKnowledge.CurrentPos + dir * FIXED_MOVE_DISTANCE;
            AILocomotionHandler.ParamGoTo param = new AILocomotionHandler.ParamGoTo
            {
                TargetPosition = destination,
                SpeedLevel = MotionFlag.Walk
            };
            taskHandler.CreateGoToTask(param);
            status = Status.Moving;
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
