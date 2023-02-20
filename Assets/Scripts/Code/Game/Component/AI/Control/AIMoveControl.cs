namespace TaoTie
{
    /// <summary>
    /// 移动
    /// </summary>
    public class AIMoveControl: AIBaseControl
    {
        private AIComponent aiComponent;
        private AIPathfindingUpdater pathfinder;
        private AIMoveControlState moveFSM;

        protected override void InitInternal()
        {
            base.InitInternal();
            aiComponent = aiKnowledge.aiOwnerEntity.GetComponent<AIComponent>();
            pathfinder = aiComponent.pathfinder;
            moveFSM = aiKnowledge.moveControlState;
        }

        public void ExecuteMove(AIDecision decision)
        {
            
        }
    }
}