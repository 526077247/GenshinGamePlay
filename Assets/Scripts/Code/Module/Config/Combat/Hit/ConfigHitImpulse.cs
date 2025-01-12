using Nino.Core;

namespace TaoTie
{
    [NinoType(false)]
    public partial class ConfigHitImpulse
    {
        [NinoMember(1)]
        public HitLevel HitLevel;
        [NinoMember(2)]
        public BaseValue HitImpulseX;
        [NinoMember(3)]
        public BaseValue HitImpulseY;
    }
}