using System.Collections.Generic;
using Nino.Core;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [LabelText("冲到面前")]
    [NinoType(false)]
    public partial class ConfigAIMeleeChargeSetting : ConfigAITacticBaseSetting
    {
        [NinoMember(10)] [NotNull]
        public ConfigAIMeleeChargeData DefaultSetting;
        [NinoMember(11)] 
        public Dictionary<int, ConfigAIMeleeChargeData> Specification = new();
    }
}