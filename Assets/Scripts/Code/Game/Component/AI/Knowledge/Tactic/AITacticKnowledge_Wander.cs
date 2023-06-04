using System.Collections.Generic;

namespace TaoTie
{
    public class AITacticKnowledge_Wander: AITacticKnowledge<ConfigAIWanderSetting, ConfigAIWanderData>
    {
        protected override ConfigAIWanderData defaultSetting => Config?.DefaultSetting;
        protected override Dictionary<int, ConfigAIWanderData> specifications => Config?.Specification;
    }
}