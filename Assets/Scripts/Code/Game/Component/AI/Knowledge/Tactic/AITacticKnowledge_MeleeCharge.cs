using System.Collections.Generic;

namespace TaoTie
{
    public class AITacticKnowledge_MeleeCharge: AITacticKnowledge<ConfigAIMeleeChargeSetting, ConfigAIMeleeChargeData>
    {
        protected override ConfigAIMeleeChargeData defaultSetting => config.DefaultSetting;
        protected override Dictionary<int, ConfigAIMeleeChargeData> specifications => config.Specification;
    }
}