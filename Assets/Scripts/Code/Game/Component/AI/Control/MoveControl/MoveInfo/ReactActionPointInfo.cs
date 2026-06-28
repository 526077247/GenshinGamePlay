namespace TaoTie
{
    /// <summary> 行动点技能：原地待机并释放对地点技能 </summary>
    public class ReactActionPointInfo : MoveInfoBase
    {
        public enum Status
        {
            Inactive = 0,
            Waiting = 1
        }

        public Status status;

        public static ReactActionPointInfo Create()
        {
            return ObjectPool.Instance.Fetch<ReactActionPointInfo>();
        }

        public override void Enter(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge, AIManager aiManager)
        {
            if (taskHandler.CurrentState == LocoTaskState.Running)
                taskHandler.CurrentState = LocoTaskState.Interrupted;
            taskHandler.UpdateMotionFlag(MotionFlag.Idle);
            status = Status.Waiting;
        }

        public override void UpdateInternal(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge, AIComponent lcai,
            AIManager aiManager)
        {
            if (status != Status.Waiting) return;
            if (aiKnowledge.SkillKnowledge.SkillsActionPoint.AvailableSkills.Count == 0)
                status = Status.Inactive;
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
