namespace TaoTie
{
    /// <summary>
    /// 移动
    /// </summary>
    public class AIMoveControl: AIBaseControl
    {
        private AIComponent aiComponent;
        private AIPathfinding pathfinder;
        
        public AIMoveControl(AIComponent ai, AIKnowledge knowledge, AIPathfinding pathfinder):base(knowledge)
        {
            aiComponent = ai;
            this.pathfinder = pathfinder;
        }


        public void ExecuteMove(AIDecision decision)
        {
            
        }
    }
}