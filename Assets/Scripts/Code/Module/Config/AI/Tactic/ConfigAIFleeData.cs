using Nino.Serialization;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigAIFleeData
    {
        [NinoMember(1)]
        public int speedLevel;
        [NinoMember(2)]
        public float turnSpeedOverride;
        [NinoMember(3)]
        public int cd;

        [NinoMember(4)][LabelText("触发逃跑的距离条件")]
        public float triggerDistance;
        [NinoMember(5)]
        public float fleeAngle;
        [NinoMember(6)][LabelText("最短逃跑距离")]
        public float fleeDistanceMin;
        [NinoMember(7)][LabelText("最远逃跑距离")]
        public float fleeDistanceMax;

        [NinoMember(8)][LabelText("逃跑完成后是否转向目标")]
        public bool turnToTarget;

        [NinoMember(9)][LabelText("是否受限于防守范围")]
        public bool restrictedByDefendArea;
        [NinoMember(10)][LabelText("是否在受阻时扩大角度范围")]
        public bool expandFleeAngleWhenBlocked;
    }
}