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
            aiComponent = aiKnowledge.aiOwnerEntity.GetComponent<AIComponent>();
            pathfinder = aiComponent.pathfinder;
            loco = new AILocomotionHandler(aiKnowledge,pathfinder);
            moveFSM = aiKnowledge.moveControlState;
        }

        public void ExecuteMove(AIDecision decision)
        {
            if (aiKnowledge.moveDecisionChanged)
            {
                switch (aiComponent.GetDecisionOld().move)
                {
                    case MoveDecision.Flee:
                        if (moveFSM.FleeInfo.status != FleeInfo.FleeStatus.Inactive)
                            break;
                        aiComponent.GetDecisionOld().move = decision.move;
                        moveFSM.Goto(decision.move, loco, aiKnowledge,aiKnowledge.aiManager);
                        break;
                    default:
                        if (loco.currentState == LocoTaskState.Running)
                            loco.currentState = LocoTaskState.Interrupted;

                        if (loco.currentState == LocoTaskState.Finished)
                        {
                            aiComponent.GetDecisionOld().move = decision.move;
                            moveFSM.Goto(decision.move, loco,aiKnowledge,aiKnowledge.aiManager);
                        }
                        break;
                }
            }
            moveFSM.UpdateMoveInfo(loco, aiKnowledge, aiComponent,aiKnowledge.aiManager);
            AITransform currentTrans = new AITransform() { pos = aiKnowledge.currentPos, fwd = aiKnowledge.currentForward };
            loco.UpdateTasks(currentTrans);
        }
    }
}