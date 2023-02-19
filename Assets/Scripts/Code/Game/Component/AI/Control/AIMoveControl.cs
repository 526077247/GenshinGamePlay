namespace TaoTie
{
    /// <summary>
    /// 移动
    /// </summary>
    public class AIMoveControl: AIBaseControl
    {
        private AIComponent aiComponent;
        private AIPathfindingUpdater pathfinder;

        protected override void InitInternal()
        {
            base.InitInternal();
            aiComponent = aiKnowledge.aiOwnerEntity.GetComponent<AIComponent>();
            pathfinder = aiComponent.pathfinder;
        }

        public void ExecuteMove(AIDecision decision)
        {
            
        }
    }
}