using Nino.Serialization;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigAIMeleeChargeData
    {
        [NinoMember(1)]
        public int speedLevel;
        [NinoMember(2)]
        public float turnSpeedOverride = 20f;
        [NinoMember(3)]
        public float startDistanceMin;
        [NinoMember(4)]
        public float startDistanceMax;
        [NinoMember(5)]
        public float stopDistance;
        [NinoMember(6)]
        public float innerDistance;
        [NinoMember(7)]
        public int speedLevelInner;
        [NinoMember(8)]
        public bool useMeleeSlot;
    }
}