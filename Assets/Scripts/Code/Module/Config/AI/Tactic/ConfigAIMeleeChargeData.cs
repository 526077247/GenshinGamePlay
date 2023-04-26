using Nino.Serialization;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigAIMeleeChargeData
    {
        [NinoMember(1)]
        public AIMoveSpeedLevel SpeedLevel;
        [NinoMember(2)]
        public float TurnSpeedOverride = 20f;
        [NinoMember(3)]
        public float StartDistanceMin;
        [NinoMember(4)]
        public float StartDistanceMax;
        [NinoMember(5)]
        public float StopDistance;
        [NinoMember(6)]
        public float InnerDistance;
        [NinoMember(7)]
        public int SpeedLevelInner;
        [NinoMember(8)]
        public bool UseMeleeSlot;
    }
}