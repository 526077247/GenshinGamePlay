using System.Collections.Generic;
using ProtoBuf;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [LabelText("逃跑")]
    [ProtoContract]
    public partial class ConfigAIFleeSetting: ConfigAITacticBaseSetting
    {
        [ProtoMember(10)][NotNull]
        public ConfigAIFleeData DefaultSetting; 
        [ProtoMember(11, IsRequired = true)]
        public Dictionary<int, ConfigAIFleeData> Specification = new Dictionary<int, ConfigAIFleeData>();
    }
}