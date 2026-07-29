using ProtoBuf;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [ProtoContract]
    public partial class ConfigAIFleeData
    {
        [ProtoMember(1)]
        public int SpeedLevel;
        [ProtoMember(2, IsRequired = true)]
        public float TurnSpeedOverride = 180;
        [ProtoMember(3)]
        public int CD;

        [ProtoMember(4)][LabelText("触发逃跑的距离条件")]
        public float TriggerDistance;
        [ProtoMember(5)]
        public float FleeAngle;
        [ProtoMember(6)][LabelText("最短逃跑距离")]
        public float FleeDistanceMin;
        [ProtoMember(7)][LabelText("最远逃跑距离")]
        public float FleeDistanceMax;

        [ProtoMember(8)][LabelText("逃跑完成后是否转向目标")]
        public bool TurnToTarget;

        [ProtoMember(9)][LabelText("是否受限于防守范围")]
        public bool RestrictedByDefendArea;
        [ProtoMember(10)][LabelText("是否在受阻时扩大角度范围")]
        public bool ExpandFleeAngleWhenBlocked;
    }
}