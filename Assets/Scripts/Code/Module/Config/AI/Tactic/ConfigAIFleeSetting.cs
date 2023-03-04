using System.Collections.Generic;
using Nino.Serialization;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [LabelText("逃跑")]
    [NinoSerialize]
    public partial class ConfigAIFleeSetting: ConfigAITacticBaseSetting
    {
        [NinoMember(10)][NotNull]
        public ConfigAIFleeData DefaultSetting; 
        [NinoMember(11)]
        public Dictionary<int, ConfigAIFleeData> Specification;
    }
}