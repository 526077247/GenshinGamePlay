using System.Collections.Generic;

namespace TaoTie
{
    public class AITacticKnowledge_MeleeCharge: AITacticKnowledge<ConfigAIMeleeChargeSetting, ConfigAIMeleeChargeData>
    {
        protected override ConfigAIMeleeChargeData defaultSetting => Config.DefaultSetting;
        protected override Dictionary<int, ConfigAIMeleeChargeData> specifications => Config.Specification;
    }
}