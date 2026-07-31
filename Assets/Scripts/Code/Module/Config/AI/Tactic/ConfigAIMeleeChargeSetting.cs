using System.Collections.Generic;
using ProtoBuf;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif

namespace TaoTie
{
    [LabelText("冲到面前")]
    [ProtoContract]
    public partial class ConfigAIMeleeChargeSetting : ConfigAITacticBaseSetting
    {
        [ProtoMember(10)] [NotNull]
        public ConfigAIMeleeChargeData DefaultSetting;
        [ProtoMember(11, IsRequired = true)] 
        public Dictionary<int, ConfigAIMeleeChargeData> Specification = new Dictionary<int, ConfigAIMeleeChargeData>();
    }
}