using System.Collections.Generic;
using ProtoBuf;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif

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