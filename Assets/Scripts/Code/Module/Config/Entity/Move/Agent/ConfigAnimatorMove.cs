using ProtoBuf;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif

namespace TaoTie
{
    [ProtoContract][LabelText("动画驱动移动")]
    public partial class ConfigAnimatorMove: ConfigMoveAgent
    {
        [ProtoMember(10, IsRequired = true)]
        public FacingMoveType FacingMove = FacingMoveType.FourDirection;
    }
}