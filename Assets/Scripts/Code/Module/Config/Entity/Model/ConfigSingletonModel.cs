using ProtoBuf;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif

namespace TaoTie
{
    [ProtoContract][LabelText("单实例")]
    public partial class ConfigSingletonModel: ConfigModel
    {
        
    }
}