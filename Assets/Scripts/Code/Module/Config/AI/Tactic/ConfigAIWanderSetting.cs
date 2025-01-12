using System.Collections.Generic;
using Nino.Core;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [LabelText("漫游")]
    [NinoType(false)]
    public partial class ConfigAIWanderSetting: ConfigAITacticBaseSetting
    {
        [NinoMember(10)] [NotNull]
        public ConfigAIWanderData DefaultSetting;
        [NinoMember(11)] 
        public Dictionary<int, ConfigAIWanderData> Specification = new();
    }
}