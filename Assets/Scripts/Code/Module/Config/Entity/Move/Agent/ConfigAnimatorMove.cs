using ProtoBuf;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [ProtoContract][LabelText("动画驱动移动")]
    public partial class ConfigAnimatorMove: ConfigMoveAgent
    {
        [ProtoMember(10, IsRequired = true)]
        public FacingMoveType FacingMove = FacingMoveType.FourDirection;
    }
}