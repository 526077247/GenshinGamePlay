using ProtoBuf;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif

namespace TaoTie
{
    [ProtoContract][LabelText("速度驱动移动")]
    public partial class ConfigSimpleMove: ConfigMoveAgent
    {
        
    }
}