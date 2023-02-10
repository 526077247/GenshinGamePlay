using UnityEngine;

namespace TaoTie
{
    public class ConfigAttackProperty
    {
        [Tooltip("伤害值")]
        public BaseValue DamagePercentage;
        [Tooltip("伤害比例")]
        public BaseValue DamagePercentageRatio;
        [Tooltip("元素类型")]
        public ElementType ElementType;
        [Tooltip("元素等级")]
        public float ElementRankRawNum;
        [Tooltip("元素量")]
        public BaseValue ElementDurability;
        [Tooltip("是否能被武器属性覆盖")]
        public bool OverrideByWeapon;
        [Tooltip("无视攻击方属性加成，这个除了无视攻击方，还会无视受击方的等级、防御。")]
        public bool IgnoreAttackerProperty;
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