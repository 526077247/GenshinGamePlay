using ProtoBuf;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [ProtoContract]
    public partial class ConfigHitPattern
    {
        [ProtoMember(1)][LabelText("击中特效")]
        public string OnHitEffectName;
        [ProtoMember(2)][LabelText("击打力度等级")][BoxGroup("击退信息")]
        public HitLevel HitLevel;
        [ProtoMember(3, IsRequired = true)][NotNull][BoxGroup("击退信息")]
        public BaseValue HitImpulseX = new SingleValue();
        [ProtoMember(4, IsRequired = true)][NotNull][BoxGroup("击退信息")]
        public BaseValue HitImpulseY = new SingleValue();
        [ProtoMember(5)][BoxGroup("击退信息")]
        public string HitImpulseType;
        [ProtoMember(6)][LabelText("冲刺中的击退数据")][BoxGroup("击退信息")]
        public ConfigHitImpulse OverrideHitImpulse;
        [ProtoMember(7)][LabelText("击退来源方向")][BoxGroup("击退信息")]
        public RetreatType RetreatType;
        [ProtoMember(8)][LabelText("击中时停(ms)")][MinValue(0)][BoxGroup("击中时停")]
        public int HitHaltTime;
        [ProtoMember(9)][ShowIf("@"+nameof(HitHaltTime)+">0")][LabelText("时停时间比例")][BoxGroup("击中时停")][Range(0,1)]
        public float HitHaltTimeScale;
        [ProtoMember(10)][ShowIf("@"+nameof(HitHaltTime)+">0")][LabelText("*被格挡时是否时停")][BoxGroup("击中时停")][Tooltip("暂用最终伤害等于0判定为格挡")]
        public bool CanBeDefenceHalt;
        [ProtoMember(11)][LabelText("击中飘字")]
        public bool MuteHitText;
        [ProtoMember(12)]
        public bool Recurring;
    }
}