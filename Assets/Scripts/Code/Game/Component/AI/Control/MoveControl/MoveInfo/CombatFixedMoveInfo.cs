using UnityEngine;

namespace TaoTie
{
    /// <summary> 战斗追击移动：朝目标最后感知位置(LKP)寻路移动，位置变化或未到达时自动重新寻路 </summary>
    public class CombatFixedMoveInfo : MoveInfoBase
    {
        public enum Status
        {
            Inactive = 0,
            Moving = 1
        }

        public Status status;
        private Vector3 lastDestination;
        private const float refreshDistanceSqr = 1.5f * 1.5f;

        public static CombatFixedMoveInfo Create()
        {
            return ObjectPool.Instance.Fetch<CombatFixedMoveInfo>();
        }

        public override void Enter(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge, AIManager aiManager)
        {
            if (status == Status.Inactive)
            {
                StartMove(taskHandler, aiKnowledge);
            }
        }

        public override void UpdateInternal(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge, AIComponent lcai,
            AIManager aiManager)
        {
            if (status != Status.Moving) return;
            if (aiKnowledge.TargetKnowledge.TargetEntity == null)
            {
                status = Status.Inactive;
                return;
            }
            var targetPos = aiKnowledge.TargetKnowledge.TargetLKP;
            //感知不到目标位置(LKP为空)时停止追击
            if (targetPos == null)
            {
                status = Status.Inactive;
                return;
            }
            var targetPosValue = targetPos.Value;
            var stopDistance = aiKnowledge.MoveKnowledge.GetAlmostReachDistance(MotionFlag.Run);
            var distance = Vector3.Distance(aiKnowledge.CurrentPos, targetPosValue);
            // 任务结束且仍未到达目的地，或目的地离上次寻路点过远时重新寻路
            if (taskHandler.CurrentState == LocoTaskState.Finished && distance > stopDistance)
            {
                StartMove(taskHandler, aiKnowledge);
            }
            else if (Vector3.SqrMagnitude(targetPosValue - lastDestination) > refreshDistanceSqr)
            {
                StartMove(taskHandler, aiKnowledge);
            }
        }

        private void StartMove(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge)
        {
            var target = aiKnowledge.TargetKnowledge.TargetEntity;
            var lkp = aiKnowledge.TargetKnowledge.TargetLKP;
            if (target == null || lkp == null)
            {
                taskHandler.UpdateMotionFlag(MotionFlag.Idle);
                status = Status.Inactive;
                return;
            }
            lastDestination = lkp.Value;
            AILocomotionHandler.ParamGoTo param = new AILocomotionHandler.ParamGoTo
            {
                TargetPosition = lastDestination,
                SpeedLevel = MotionFlag.Run,
                UseNavmesh = NavMeshUseType.Auto
            };
            taskHandler.CreateGoToTask(param);
            status = Status.Moving;
        }

        public override void Leave(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge, AIManager aiManager)
        {
            base.Leave(taskHandler, aiKnowledge, aiManager);
            if (taskHandler.CurrentState == LocoTaskState.Running)
                taskHandler.CurrentState = LocoTaskState.Interrupted;
            status = Status.Inactive;
        }

        public override void Dispose()
        {
            status = Status.Inactive;
            lastDestination = default;
            ObjectPool.Instance.Recycle(this);
        }
    }
}