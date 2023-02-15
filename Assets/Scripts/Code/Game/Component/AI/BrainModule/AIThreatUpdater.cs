namespace TaoTie
{
    /// <summary>
    /// 威胁模块
    /// </summary>
    public class AIThreatUpdater: BrainModuleBase
    {
        private AIComponent aiComponent;
        public AIThreatUpdater(AIComponent aiComponent)
        {
            this.aiComponent = aiComponent;
        }
    }
}