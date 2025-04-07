using System.Collections.Generic;
using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [NinoType(false)]
    public partial class ConfigAttackProperty
    {
        [NinoMember(1)][LabelText("伤害值")] [NotNull]
        public BaseValue DamagePercentage = new SingleValue();
        [NinoMember(2)][LabelText("*伤害比例(小数)")] [NotNull][Tooltip("小数,1为100%")]
        public BaseValue DamagePercentageRatio = new SingleValue(1);
        [NinoMember(3)][LabelText("击打类型")]
        public StrikeType StrikeType;
        [NinoMember(4)][LabelText("破霸体值")] 
        public Dictionary<HitBoxType, BaseValue> EnBreak = new Dictionary<HitBoxType, BaseValue>();
        [NinoMember(5)][LabelText("攻击类型")]
        public AttackType AttackType;
        [NinoMember(6)][LabelText("额外伤害值")][NotNull]
        public BaseValue DamageExtra = new ZeroValue();
        [NinoMember(7)][LabelText("*暴击率(小数)")][NotNull][Tooltip("小数,1为100%")]
        public BaseValue BonusCritical = new ZeroValue();
        [NinoMember(8)][LabelText("*暴击伤害(小数)")][NotNull][Tooltip("小数,1为100%")]
        public BaseValue BonusCriticalHurt = new SingleValue(1);
        [NinoMember(9)][LabelText("忽略等级差距带来的衰减")]
        public bool IgnoreLevelDiff;
        [NinoMember(10)][LabelText("是否真伤")]
        public bool TrueDamage;
    }
}