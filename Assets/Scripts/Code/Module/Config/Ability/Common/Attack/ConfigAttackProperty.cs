namespace TaoTie
{
    public class ConfigAttackProperty
    {
        /// <summary>
        /// 伤害值
        /// </summary>
        public BaseValue DamagePercentage;
        /// <summary>
        /// 伤害比例
        /// </summary>
        public BaseValue DamagePercentageRatio;
        /// <summary>
        /// 元素类型
        /// </summary>
        public ElementType ElementType;
        /// <summary>
        /// 元素等级
        /// </summary>
        public float ElementRankRawNum;
        /// <summary>
        /// 元素量
        /// </summary>
        public BaseValue ElementDurability;
        /// <summary>
        /// 是否能被武器属性覆盖
        /// </summary>
        public bool OverrideByWeapon;
        /// <summary>
        /// 无视攻击方属性加成，这个除了无视攻击方，还会无视受击方的等级、防御。
        /// </summary>
        public bool IgnoreAttackerProperty;
        /// <summary>
        /// 击打类型
        /// </summary>
        public StrikeType StrikeType;
        /// <summary>
        /// 破霸体值
        /// </summary>
        public float EnBreakRawNum;
        /// <summary>
        /// 击中头部破霸体值
        /// </summary>
        public float EnHeadBreakRawNum;
        /// <summary>
        /// 攻击类型
        /// </summary>
        public AttackType AttackType;
        /// <summary>
        /// 额外伤害值
        /// </summary>
        public BaseValue DamageExtra;
        /// <summary>
        /// 暴击率
        /// </summary>
        public BaseValue BonusCritical;
        /// <summary>
        /// 暴击伤害
        /// </summary>
        public BaseValue BonusCriticalHurt;
        /// <summary>
        /// 忽略等级差距带来的衰减
        /// </summary>
        public bool IgnoreLevelDiff;
        /// <summary>
        /// 是否真伤
        /// </summary>
        public bool TrueDamage;
    }
}