using ProtoBuf;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif

namespace TaoTie
{
    [LabelText("返回出生点")]
    [ProtoContract]
    public partial class ConfigAIReturnToBornPosSetting: ConfigAITacticBaseSetting
    {

    }
}