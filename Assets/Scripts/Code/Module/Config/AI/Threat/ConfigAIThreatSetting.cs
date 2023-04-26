using Nino.Serialization;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigAIThreatSetting
    {
        [LabelText("启用")]
        [NinoMember(1)]
        public bool Enable = true;
        [NinoMember(2)]
        public float ClearThreatTargetDistance;
        [NinoMember(3)]
        public float ClearThreatEdgeDistance;
        [NinoMember(4)]
        public bool ClearThreatByLostPath;
        [NinoMember(5)]
        public bool ClearThreatByTargetOutOfZone;
        [NinoMember(6)]
        public int ClearThreatTimerByDistance;
        [NinoMember(7)]
        public int ClearThreatTimerByLostPath;
        [NinoMember(8)]
        public int ClearThreatTimerByTargetOutOfZone;
        [NinoMember(9)]
        public float ViewThreatGrow = 100f;
        [NinoMember(10)]
        public float HearThreatGrow;
        [NinoMember(11)]
        public float FeelThreatGrow = 500f;
        [NinoMember(12)]
        public float ThreatDecreaseSpeed = 30f;
        [NinoMember(13)]
        public float ThreatBroadcastRange;
        [NinoMember(14)][LabelText("视觉感知衰减曲线")]
        public AIPoint[] ViewAttenuation;
        [NinoMember(15)][LabelText("听觉感知衰减曲线")]
        public AIPoint[] HearAttenuation;
    }
}