using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigHitPattern
    {
        [NinoMember(1)][LabelText("击中特效")]
        public string OnHitEffectName;
        [NinoMember(2)][LabelText("击打力度等级")]
        public HitLevel HitLevel;
        [NinoMember(3)][NotNull]
        public BaseValue HitImpulseX;
        [NinoMember(4)][NotNull]
        public BaseValue HitImpulseY;
        [NinoMember(5)]
        public string HitImpulseType;
        [NinoMember(6)][LabelText("冲刺中的击退数据")]
        public ConfigHitImpulse OverrideHitImpulse;
        [NinoMember(7)][LabelText("击退方向")]
        public RetreatType RetreatType;
        [NinoMember(8)]
        public float HitHaltTime;
        [NinoMember(9)]
        public float HitHaltTimeScale;
        [NinoMember(10)]
        public bool CanBeDefenceHalt;
        [NinoMember(11)]
        public bool MuteHitText;
        [NinoMember(12)]
        public bool Recurring;
    }
}