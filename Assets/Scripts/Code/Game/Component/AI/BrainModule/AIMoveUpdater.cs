namespace TaoTie
{
    /// <summary>
    /// 移动
    /// </summary>
    public class AIMoveUpdater : BrainModuleBase
    {
        private FsmComponent fsm => knowledge.Entity.GetComponent<FsmComponent>();
        
        protected override void UpdateMainThreadInternal()
        {
            base.UpdateMainThreadInternal();
            if (knowledge.MoveKnowledge.Config!=null && knowledge.MoveKnowledge.Config.Enable)
            {
                knowledge.MoveKnowledge.CanMove = fsm.DefaultFsm.CurrentState.CanMove;
                knowledge.MoveKnowledge.CanTurn = fsm.DefaultFsm.CurrentState.CanTurn;
            }
            else
            {
                knowledge.MoveKnowledge.CanMove = false;
                knowledge.MoveKnowledge.CanTurn = false;
            }
        }
    }
}