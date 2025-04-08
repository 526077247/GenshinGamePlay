using System.Collections.Generic;
using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [NinoType(false)]
    public partial class ConfigAttackProperty
    {
        [NinoMember(1)][LabelText("*伤害值")] [NotNull]
        [Tooltip("受暴击、伤害比例、等级差距衰减等所有因素影响,最终结果最低为0不会为负数,即不可用于实现生命回复功能")]
        public BaseValue DamagePercentage = new SingleValue();
        [NinoMember(2)][LabelText("*伤害比例(小数)")] [NotNull][Tooltip("小数,1为100%")]
        public BaseValue DamagePercentageRatio = new SingleValue(1);
        [NinoMember(3)][LabelText("击打类型")]
        public StrikeType StrikeType;
        [NinoMember(4)][LabelText("破霸体值")] 
        public Dictionary<HitBoxType, BaseValue> EnBreak = new Dictionary<HitBoxType, BaseValue>();
        [NinoMember(5)][LabelText("攻击类型")]
        public AttackType AttackType;
        [NinoMember(6)][LabelText("*额外伤害值")][NotNull][Tooltip("暂定为不受任何因素影响的固定伤害,如默认可以给1,防止伤害最终结算为0")]
        public BaseValue DamageExtra = new SingleValue(1);
        [NinoMember(7)][LabelText("*暴击率(小数)")][NotNull][Tooltip("小数,1为100%")]
        public BaseValue BonusCritical = new ZeroValue();
        [NinoMember(8)][LabelText("*额外暴击伤害(小数)")][NotNull]
        [Tooltip("小数,不考虑其他因素时默认暴击伤害系数为1+该设定值. 例：未暴击伤害为100,当该值为0时暴击伤害为100x(1+0)=100,当该值为0.5时暴击伤害为100x(1+0.5)=150")]
        public BaseValue BonusCriticalHurt = new SingleValue(1);
        [NinoMember(9)][LabelText("*忽略等级差距带来的衰减")]
        [Tooltip("默认等级差系数范围(0,2)公式为: Fx双曲正切值((攻击方等级-防御方等级)/10) + 1")]
        public bool IgnoreLevelDiff;
        [NinoMember(10)][LabelText("*是否真伤")] [Tooltip("真伤可以忽视对方防御影响,默认防御减伤比例系数范围(0,1)公式为：(攻击方等级*100)/(防守方防御值+攻击方等级*100)")]
        public bool TrueDamage;
    }
}