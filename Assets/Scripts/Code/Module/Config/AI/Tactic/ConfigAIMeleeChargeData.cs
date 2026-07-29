using ProtoBuf;

namespace TaoTie
{
    [ProtoContract]
    public partial class ConfigAIMeleeChargeData
    {
        [ProtoMember(1)]
        public MotionFlag SpeedLevel;
        [ProtoMember(2, IsRequired = true)]
        public float TurnSpeedOverride = 20f;
        [ProtoMember(3)]
        public float StartDistanceMin;
        [ProtoMember(4)]
        public float StartDistanceMax;
        [ProtoMember(5)]
        public float StopDistance;
        [ProtoMember(6)]
        public float InnerDistance;
        [ProtoMember(7, IsRequired = true)]
        public MotionFlag SpeedLevelInner = MotionFlag.Run;
        [ProtoMember(8)]
        public bool UseMeleeSlot;
    }
}