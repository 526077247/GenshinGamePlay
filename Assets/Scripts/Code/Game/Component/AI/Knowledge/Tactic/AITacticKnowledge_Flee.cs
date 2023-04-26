using System.Collections.Generic;

namespace TaoTie
{
    public class AITacticKnowledge_Flee: AITacticKnowledge<ConfigAIFleeSetting, ConfigAIFleeData>
    {
        protected override ConfigAIFleeData defaultSetting => Config.DefaultSetting;
        protected override Dictionary<int, ConfigAIFleeData> specifications => Config.Specification;
    }
}