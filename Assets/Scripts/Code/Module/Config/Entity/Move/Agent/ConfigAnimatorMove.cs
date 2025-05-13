using Nino.Core;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [NinoType(false)][LabelText("动画驱动移动")]
    public partial class ConfigAnimatorMove: ConfigMoveAgent
    {
        [NinoMember(10)]
        public FacingMoveType FacingMove = FacingMoveType.FourDirection;
    }
}