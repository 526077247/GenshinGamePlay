using System.Collections.Generic;

namespace TaoTie
{
    public sealed class AITacticKnowledge_FacingMove: AITacticKnowledge<ConfigAIFacingMoveSetting, ConfigAIFacingMoveData>
    {
        protected override ConfigAIFacingMoveData defaultSetting => config.DefaultSetting;
        protected override Dictionary<int, ConfigAIFacingMoveData> specifications => config.Specification;
    }
}