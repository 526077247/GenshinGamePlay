using System.Collections.Generic;
using ProtoBuf;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [LabelText("漫游")]
    [ProtoContract]
    public partial class ConfigAIWanderSetting: ConfigAITacticBaseSetting
    {
        [ProtoMember(10)] [NotNull]
        public ConfigAIWanderData DefaultSetting;
        [ProtoMember(11, IsRequired = true)] 
        public Dictionary<int, ConfigAIWanderData> Specification = new Dictionary<int, ConfigAIWanderData>();
    }
}