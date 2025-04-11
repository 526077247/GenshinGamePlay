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
            if (knowledge.MoveDecisionChanged)
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
                        if (loco.CurrentState == LocoTaskState.Running)
                            loco.CurrentState = LocoTaskState.Interrupted;

                        if (loco.CurrentState != LocoTaskState.Running)
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