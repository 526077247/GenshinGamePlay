using Nino.Serialization;

namespace TaoTie
{
    [NinoSerialize]
    public class ConfigHitImpulse
    {
        [NinoMember(1)]
        public HitLevel HitLevel;
        [NinoMember(2)]
        public BaseValue HitImpulseX;
        [NinoMember(3)]
        public BaseValue HitImpulseY;
    }
}