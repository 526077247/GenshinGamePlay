using System.Collections.Generic;
using Nino.Serialization;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [LabelText("漫游")]
    [NinoSerialize]
    public partial class ConfigAIWanderSetting: ConfigAITacticBaseSetting
    {
        [NinoMember(10)] [NotNull]
        public ConfigAIWanderData DefaultSetting;
        [NinoMember(11)] 
        public Dictionary<int, ConfigAIWanderData> Specification;
    }
}