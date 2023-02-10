using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    public class ConfigAttackProperty
    {
        [Tooltip("伤害值")] [NotNull]
        public BaseValue DamagePercentage;
        [Tooltip("伤害比例")] [NotNull]
        public BaseValue DamagePercentageRatio;
        [Tooltip("击打类型")]
        public StrikeType StrikeType;
        [Tooltip("破霸体值")] 
        public Dictionary<HitBoxType, BaseValue> EnBreak;
        [Tooltip("攻击类型")]
        public AttackType AttackType;
        [Tooltip("额外伤害值")] [NotNull]
        public BaseValue DamageExtra;
        [Tooltip("暴击率")] [NotNull]
        public BaseValue BonusCritical;
        [Tooltip("暴击伤害")] [NotNull]
        public BaseValue BonusCriticalHurt;
        [Tooltip("忽略等级差距带来的衰减")]
        public bool IgnoreLevelDiff;
        [Tooltip("是否真伤")]
        public bool TrueDamage;
    }
}