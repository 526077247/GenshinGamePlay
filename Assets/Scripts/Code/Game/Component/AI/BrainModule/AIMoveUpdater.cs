namespace TaoTie
{
    /// <summary>
    /// 移动
    /// </summary>
    public class AIMoveUpdater : BrainModuleBase
    {
        private FsmComponent fsm => knowledge.aiOwnerEntity.GetComponent<FsmComponent>();
        
        protected override void UpdateMainThreadInternal()
        {
            base.UpdateMainThreadInternal();
            knowledge.moveKnowledge.canMove = fsm.DefaultFsm.currentState.CanMove;
            knowledge.moveKnowledge.canTurn = fsm.DefaultFsm.currentState.CanTurn;
        }
    }
}