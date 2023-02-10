using UnityEngine;

namespace TaoTie
{
    public class ConfigAttackProperty
    {
        [Tooltip("伤害值")]
        public BaseValue DamagePercentage;
        [Tooltip("伤害比例")]
        public BaseValue DamagePercentageRatio;
        [Tooltip("击打类型")]
        public StrikeType StrikeType;
        [Tooltip("破霸体值")]
        public float EnBreakRawNum;
        [Tooltip("击中头部破霸体值")]
        public float EnHeadBreakRawNum;
        [Tooltip("攻击类型")]
        public AttackType AttackType;
        [Tooltip("额外伤害值")]
        public BaseValue DamageExtra;
        [Tooltip("暴击率")]
        public BaseValue BonusCritical;
        [Tooltip("暴击伤害")]
        public BaseValue BonusCriticalHurt;
        [Tooltip("忽略等级差距带来的衰减")]
        public bool IgnoreLevelDiff;
        [Tooltip("是否真伤")]
        public bool TrueDamage;
    }
}