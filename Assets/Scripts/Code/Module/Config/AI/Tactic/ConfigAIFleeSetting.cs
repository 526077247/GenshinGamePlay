using System.Collections.Generic;
using Nino.Core;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [LabelText("逃跑")]
    [NinoType(false)]
    public partial class ConfigAIFleeSetting: ConfigAITacticBaseSetting
    {
        [NinoMember(10)][NotNull]
        public ConfigAIFleeData DefaultSetting; 
        [NinoMember(11)]
        public Dictionary<int, ConfigAIFleeData> Specification = new Dictionary<int, ConfigAIFleeData>();
    }
}