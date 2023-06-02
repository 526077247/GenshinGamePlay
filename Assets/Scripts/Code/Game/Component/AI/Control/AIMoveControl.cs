namespace TaoTie
{
    /// <summary>
    /// 移动
    /// </summary>
    public class AIMoveControl: AIBaseControl
    {
        private AIComponent aiComponent;
        private AILocomotionHandler loco;
        private AIPathfindingUpdater pathfinder;
        private AIMoveControlState moveFSM;

        protected override void InitInternal()
        {
            base.InitInternal();
            aiComponent = aiKnowledge.AiOwnerEntity.GetComponent<AIComponent>();
            pathfinder = aiComponent.Pathfinder;
            loco = new AILocomotionHandler(aiKnowledge,pathfinder);
            moveFSM = aiKnowledge.MoveControlState;
        }

        public void ExecuteMove(AIDecision decision)
        {
            if (aiKnowledge.MoveDecisionChanged)
            {
                switch (aiComponent.GetDecisionOld().Move)
                {
                    case MoveDecision.Flee:
                        if (moveFSM.FleeInfo.Status != FleeInfo.FleeStatus.Inactive)
                            break;
                        aiComponent.GetDecisionOld().Move = decision.Move;
                        moveFSM.Goto(decision.Move, loco, aiKnowledge,aiKnowledge.AiManager);
                        break;
                    default:
                        if (loco.currentState == LocoTaskState.Running)
                            loco.currentState = LocoTaskState.Interrupted;

                        if (loco.currentState == LocoTaskState.Finished)
                        {
                            aiComponent.GetDecisionOld().Move = decision.Move;
                            moveFSM.Goto(decision.Move, loco,aiKnowledge,aiKnowledge.AiManager);
                        }
                        break;
                }
            }
            moveFSM.UpdateMoveInfo(loco, aiKnowledge, aiComponent,aiKnowledge.AiManager);
            AITransform currentTrans = new AITransform() { pos = aiKnowledge.CurrentPos, fwd = aiKnowledge.CurrentForward };
            loco.UpdateTasks(currentTrans);
        }
    }
}