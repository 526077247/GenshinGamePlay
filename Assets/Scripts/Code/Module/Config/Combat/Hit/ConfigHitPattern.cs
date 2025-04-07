using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [NinoType(false)]
    public partial class ConfigHitPattern
    {
        [NinoMember(1)][LabelText("击中特效")]
        public string OnHitEffectName;
        [NinoMember(2)][LabelText("击打力度等级")][BoxGroup("击退信息")]
        public HitLevel HitLevel;
        [NinoMember(3)][NotNull][BoxGroup("击退信息")]
        public BaseValue HitImpulseX = new SingleValue();
        [NinoMember(4)][NotNull][BoxGroup("击退信息")]
        public BaseValue HitImpulseY = new SingleValue();
        [NinoMember(5)][BoxGroup("击退信息")]
        public string HitImpulseType;
        [NinoMember(6)][LabelText("冲刺中的击退数据")][BoxGroup("击退信息")]
        public ConfigHitImpulse OverrideHitImpulse;
        [NinoMember(7)][LabelText("击退来源方向")][BoxGroup("击退信息")]
        public RetreatType RetreatType;
        [NinoMember(8)][LabelText("击中时停(ms)")][MinValue(0)][BoxGroup("击中时停")]
        public int HitHaltTime;
        [NinoMember(9)][ShowIf("@"+nameof(HitHaltTime)+">0")][LabelText("时停时间比例")][BoxGroup("击中时停")][Range(0,1)]
        public float HitHaltTimeScale;
        [NinoMember(10)][ShowIf("@"+nameof(HitHaltTime)+">0")][LabelText("被格挡时是否时停")][BoxGroup("击中时停")]
        public bool CanBeDefenceHalt;
        [NinoMember(11)][LabelText("击中飘字")]
        public bool MuteHitText;
        [NinoMember(12)]
        public bool Recurring;
    }
}