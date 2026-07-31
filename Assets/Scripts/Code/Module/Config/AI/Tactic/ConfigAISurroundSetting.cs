using ProtoBuf;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif

namespace TaoTie
{
    [LabelText("环绕对峙")]
    [ProtoContract]
    public partial class ConfigAISurroundSetting: ConfigAITacticBaseSetting
    {

    }
}