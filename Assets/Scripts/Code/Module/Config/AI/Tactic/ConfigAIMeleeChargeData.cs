using Nino.Core;

namespace TaoTie
{
    [NinoType(false)]
    public partial class ConfigAIMeleeChargeData
    {
        [NinoMember(1)]
        public MotionFlag SpeedLevel;
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
        public MotionFlag SpeedLevelInner = MotionFlag.Run;
        [NinoMember(8)]
        public bool UseMeleeSlot;
    }
}