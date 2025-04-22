using Nino.Core;

namespace TaoTie
{
    [NinoType(false)]
    public class ConfigAnimatorMove: ConfigMoveStrategy
    {
        [NinoMember(10)]
        public FacingMoveType FacingMove = FacingMoveType.FourDirection;
    }
}