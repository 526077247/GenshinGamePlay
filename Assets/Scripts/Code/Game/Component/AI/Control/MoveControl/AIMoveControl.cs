namespace TaoTie
{
    /// <summary>
    /// 移动
    /// </summary>
    public class AIMoveControl: AIBaseControl
    {
        private AIComponent aiComponent;
        private AILocomotionHandler loco;
        private AIMoveControlState moveFSM;

        protected override void InitInternal()
        {
            aiComponent = knowledge.Entity.GetComponent<AIComponent>();
            loco = new AILocomotionHandler(knowledge);
            moveFSM = knowledge.MoveControlState;
        }

        public void ExecuteMove(AIDecision decision)
        {
            if (knowledge.MoveDecisionChanged || loco.currentState == LocoTaskState.Finished)
            {
                switch (aiComponent.GetDecisionOld().Move)
                {
                    case MoveDecision.Flee:
                        if (moveFSM.FleeInfo.Status != FleeInfo.FleeStatus.Inactive)
                            break;
                        knowledge.MoveDecisionChanged = false;
                        moveFSM.Goto(decision.Move, loco, knowledge,knowledge.AIManager);
                        break;
                    default:
                        if (loco.currentState == LocoTaskState.Running)
                            loco.currentState = LocoTaskState.Interrupted;

                        if (loco.currentState == LocoTaskState.Finished)
                        {
                            knowledge.MoveDecisionChanged = false;
                            moveFSM.Goto(decision.Move, loco,knowledge,knowledge.AIManager);
                        }
                        break;
                }
            }
            moveFSM.UpdateMoveInfo(loco, knowledge, aiComponent,knowledge.AIManager);
            AITransform currentTrans = new AITransform() { Position = knowledge.CurrentPos, Forward = knowledge.CurrentForward };
            loco.UpdateTasks(currentTrans);
        }
    }
}