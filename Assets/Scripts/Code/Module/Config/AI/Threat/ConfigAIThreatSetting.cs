using Nino.Serialization;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigAIThreatSetting
    {
        [LabelText("启用")]
        [NinoMember(1)]
        public bool enable;
        [NinoMember(2)]
        public float clearThreatTargetDistance;
        [NinoMember(3)]
        public float clearThreatEdgeDistance;
        [NinoMember(4)]
        public bool clearThreatByLostPath;
        [NinoMember(5)]
        public bool clearThreatByTargetOutOfZone;
        [NinoMember(6)]
        public int clearThreatTimerByDistance;
        [NinoMember(7)]
        public int clearThreatTimerByLostPath;
        [NinoMember(8)]
        public int clearThreatTimerByTargetOutOfZone;
        [NinoMember(9)]
        public float viewThreatGrow = 100f;
        [NinoMember(10)]
        public float hearThreatGrow;
        [NinoMember(11)]
        public float feelThreatGrow = 500f;
        [NinoMember(12)]
        public float threatDecreaseSpeed = 30f;
        [NinoMember(13)]
        public float threatBroadcastRange;
        [NinoMember(14)][LabelText("视觉感知衰减曲线")]
        public AIPoint[] viewAttenuation;
        [NinoMember(15)][LabelText("听觉感知衰减曲线")]
        public AIPoint[] hearAttenuation;
    }
}