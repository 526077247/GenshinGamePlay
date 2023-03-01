using System.Collections.Generic;

namespace TaoTie
{
    public class AITacticKnowledge_Wander: AITacticKnowledge<ConfigAIWanderSetting, ConfigAIWanderData>
    {
        protected override ConfigAIWanderData defaultSetting => config.DefaultSetting;
        protected override Dictionary<int, ConfigAIWanderData> specifications => config.Specification;
    }
}