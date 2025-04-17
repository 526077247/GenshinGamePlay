using Nino.Core;

namespace TaoTie
{
    [NinoType(false)]
    public class ConfigAnimatorMove: ConfigMove
    {
        [NinoMember(10)]
        public FacingMoveType FacingMove = FacingMoveType.FourDirection;
    }
}