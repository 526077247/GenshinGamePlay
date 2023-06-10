namespace TaoTie
{
    public class StandStillInfo: MoveInfoBase
    {

        public static StandStillInfo Create()
        {
            return ObjectPool.Instance.Fetch<StandStillInfo>();
        }
        public override void Dispose()
        {
            ObjectPool.Instance.Recycle(this);
        }

        public override void Enter(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge, AIManager aiManager)
        {
            base.Enter(taskHandler, aiKnowledge, aiManager);
            if(taskHandler.currentState == LocoTaskState.Running)
                taskHandler.currentState = LocoTaskState.Interrupted;
            taskHandler.UpdateMotionFlag(MotionFlag.Idle);
        }

        public override void UpdateInternal(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge, AIComponent lcai, AIManager aiManager)
        {

        }
    }
}