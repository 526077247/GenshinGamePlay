using System.Collections.Generic;
using ProtoBuf;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif

namespace TaoTie
{
    [LabelText("前后左右随机移动")]
    [ProtoContract]
    public partial class ConfigAIFacingMoveSetting: ConfigAITacticBaseSetting
    {
        [ProtoMember(10)][NotNull]
        public ConfigAIFacingMoveData DefaultSetting;
        [ProtoMember(11, IsRequired = true)]
        public Dictionary<int, ConfigAIFacingMoveData> Specification = new Dictionary<int, ConfigAIFacingMoveData>();
    }
}