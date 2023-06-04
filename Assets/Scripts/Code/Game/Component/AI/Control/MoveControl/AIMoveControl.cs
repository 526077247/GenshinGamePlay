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
            base.InitInternal();
            aiComponent = knowledge.Entity.GetComponent<AIComponent>();
            loco = new AILocomotionHandler(knowledge);
            moveFSM = knowledge.MoveControlState;
        }

        public void ExecuteMove(AIDecision decision)
        {
            if (knowledge.MoveDecisionChanged)
            {
                switch (aiComponent.GetDecisionOld().Move)
                {
                    case MoveDecision.Flee:
                        if (moveFSM.FleeInfo.Status != FleeInfo.FleeStatus.Inactive)
                            break;
                        knowledge.MoveDecisionChanged = false;
                        moveFSM.Goto(decision.Move, loco, knowledge,knowledge.AiManager);
                        break;
                    default:
                        if (loco.currentState == LocoTaskState.Running)
                            loco.currentState = LocoTaskState.Interrupted;

                        if (loco.currentState == LocoTaskState.Finished)
                        {
                            knowledge.MoveDecisionChanged = false;
                            moveFSM.Goto(decision.Move, loco,knowledge,knowledge.AiManager);
                        }
                        break;
                }
            }
            moveFSM.UpdateMoveInfo(loco, knowledge, aiComponent,knowledge.AiManager);
            AITransform currentTrans = new AITransform() { pos = knowledge.CurrentPos, fwd = knowledge.CurrentForward };
            loco.UpdateTasks(currentTrans);
        }
    }
}