using UnityEngine;

namespace TaoTie
{
    /// <summary> 脚本移动到指定位置 </summary>
    public class ScriptedMoveToInfo : MoveInfoBase
    {
        public enum Status
        {
            Inactive = 0,
            Moving = 1
        }

        public Status status;

        public static ScriptedMoveToInfo Create()
        {
            return ObjectPool.Instance.Fetch<ScriptedMoveToInfo>();
        }

        public override void Enter(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge, AIManager aiManager)
        {
            StartMove(taskHandler, aiKnowledge);
        }

        public override void UpdateInternal(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge, AIComponent lcai,
            AIManager aiManager)
        {
            if (status == Status.Moving && taskHandler.CurrentState == LocoTaskState.Finished)
                status = Status.Inactive;
        }

        private void StartMove(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge)
        {
            if (aiKnowledge.TargetKnowledge.TargetType != AITargetType.PointTarget)
            {
                status = Status.Inactive;
                return;
            }
            AILocomotionHandler.ParamGoTo param = new AILocomotionHandler.ParamGoTo
            {
                TargetPosition = aiKnowledge.TargetKnowledge.TargetPosition,
                SpeedLevel = MotionFlag.Run
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
