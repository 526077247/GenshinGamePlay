using System.Collections.Generic;
using Nino.Serialization;
using UnityEngine;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigAttackProperty
    {
        [NinoMember(1)][Tooltip("伤害值")] [NotNull]
        public BaseValue DamagePercentage;
        [NinoMember(2)][Tooltip("伤害比例")] [NotNull]
        public BaseValue DamagePercentageRatio;
        [NinoMember(3)][Tooltip("击打类型")]
        public StrikeType StrikeType;
        [NinoMember(4)][Tooltip("破霸体值")] 
        public Dictionary<HitBoxType, BaseValue> EnBreak;
        [NinoMember(5)][Tooltip("攻击类型")]
        public AttackType AttackType;
        [NinoMember(6)][Tooltip("额外伤害值")] [NotNull]
        public BaseValue DamageExtra;
        [NinoMember(7)][Tooltip("暴击率")] [NotNull]
        public BaseValue BonusCritical;
        [NinoMember(8)][Tooltip("暴击伤害")] [NotNull]
        public BaseValue BonusCriticalHurt;
        [NinoMember(9)][Tooltip("忽略等级差距带来的衰减")]
        public bool IgnoreLevelDiff;
        [NinoMember(10)][Tooltip("是否真伤")]
        public bool TrueDamage;
    }
}