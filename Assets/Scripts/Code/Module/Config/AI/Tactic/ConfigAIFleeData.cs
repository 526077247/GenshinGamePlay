using Nino.Core;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [NinoType(false)]
    public partial class ConfigAIFleeData
    {
        [NinoMember(1)]
        public int SpeedLevel;
        [NinoMember(2)]
        public float TurnSpeedOverride = 180;
        [NinoMember(3)]
        public int CD;

        [NinoMember(4)][LabelText("触发逃跑的距离条件")]
        public float TriggerDistance;
        [NinoMember(5)]
        public float FleeAngle;
        [NinoMember(6)][LabelText("最短逃跑距离")]
        public float FleeDistanceMin;
        [NinoMember(7)][LabelText("最远逃跑距离")]
        public float FleeDistanceMax;

        [NinoMember(8)][LabelText("逃跑完成后是否转向目标")]
        public bool TurnToTarget;

        [NinoMember(9)][LabelText("是否受限于防守范围")]
        public bool RestrictedByDefendArea;
        [NinoMember(10)][LabelText("是否在受阻时扩大角度范围")]
        public bool ExpandFleeAngleWhenBlocked;
    }
}