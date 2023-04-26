using System.Collections.Generic;

namespace TaoTie
{
    public sealed class AITacticKnowledge_FacingMove: AITacticKnowledge<ConfigAIFacingMoveSetting, ConfigAIFacingMoveData>
    {
        protected override ConfigAIFacingMoveData defaultSetting => Config.DefaultSetting;
        protected override Dictionary<int, ConfigAIFacingMoveData> specifications => Config.Specification;
    }
}