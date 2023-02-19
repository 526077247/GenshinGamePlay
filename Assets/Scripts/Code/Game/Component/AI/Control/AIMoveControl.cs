namespace TaoTie
{
    /// <summary>
    /// 移动
    /// </summary>
    public class AIMoveControl: AIBaseControl
    {
        private AIComponent aiComponent;
        private AIPathfindingUpdater pathfinder;
        
        public AIMoveControl(AIComponent ai, AIKnowledge knowledge, AIPathfindingUpdater pathfinder):base(knowledge)
        {
            aiComponent = ai;
            this.pathfinder = pathfinder;
        }


        public void ExecuteMove(AIDecision decision)
        {
            
        }
    }
}