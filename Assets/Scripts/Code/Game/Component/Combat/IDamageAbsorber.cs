namespace TaoTie
{
    /// <summary>
    /// 伤害吸收者：在最终伤害算出之后、扣血之前，按优先级依次抵扣本次伤害。
    ///
    /// 谁实现它：<see cref="AbsorbByStateMixin"/>。做成接口而不是让 <see cref="AttackResult"/>
    /// 直接持有具体 Mixin，是为了让「吸收」这件事与「数据存在哪」解耦 ——
    /// 现在是修饰器状态池，以后换成伤害延迟结算、伤害转移也不用动 <see cref="AttackResult"/>。
    /// </summary>
    public interface IDamageAbsorber
    {
        /// <summary>
        /// 结算顺序，数值小的先扣。
        /// 专盾（只吃某一类伤害）应该排在通用盾前面，否则通用盾会先把伤害吃光，
        /// 专盾永远等不到属于自己的那一下。
        /// </summary>
        int AbsorbPriority { get; }

        /// <summary>
        /// 吸收 <paramref name="damage"/> 点伤害，返回<b>实际</b>吸收量（0 = 没挡住，没盾时也要返回 0）。
        /// 实现方必须允许「吸收量 &lt; 伤害量」，不能私自把未吸收的部分也吞掉。
        /// </summary>
        int AbsorbDamage(int damage);
    }
}
