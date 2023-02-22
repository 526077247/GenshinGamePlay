using System.Collections.Generic;
using Nino.Serialization;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [LabelText("冲到面前")]
    [NinoSerialize]
    public partial class ConfigAIMeleeChargeSetting : ConfigAITacticBaseSetting
    {
        [NinoMember(10)] 
        public ConfigAIMeleeChargeData DefaultSetting;
        [NinoMember(11)] 
        public Dictionary<int, ConfigAIMeleeChargeData> Specification;
    }
}