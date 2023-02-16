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
        public float clearThreatTargetDistance;
        public float clearThreatEdgeDistance;
        public bool clearThreatByLostPath;
        public bool clearThreatByTargetOutOfZone;
        public float clearThreatTimerByDistance;
        public float clearThreatTimerByLostPath;
        public float clearThreatTimerByTargetOutOfZone;
        public float viewThreatGrow = 100f;
        public float hearThreatGrow;
        public float feelThreatGrow = 500f;
        public float threatDecreaseSpeed = 30f;
        public float threatBroadcastRange;
        //视觉感知衰减曲线
        public AIMath.AIPoint[] viewAttenuation;
        //听觉感知衰减曲线
        public AIMath.AIPoint[] hearAttenuation;
    }
}