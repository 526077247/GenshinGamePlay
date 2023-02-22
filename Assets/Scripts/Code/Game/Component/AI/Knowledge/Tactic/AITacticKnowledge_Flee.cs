using System.Collections.Generic;

namespace TaoTie
{
    public class AITacticKnowledge_Flee: AITacticKnowledge<ConfigAIFleeSetting, ConfigAIFleeData>
    {
        protected override ConfigAIFleeData defaultSetting => config.DefaultSetting;
        protected override Dictionary<int, ConfigAIFleeData> specifications => config.Specification;
    }
}