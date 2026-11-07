#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif

namespace TaoTie
{
    /// <summary>
    /// 读层数时，"这一层是谁身上的"。
    ///
    /// 为什么必须显式选：求值时手上只有三样东西 —— 攻击者实体、Ability、Modifier，
    /// 而这三者指向的实体在实战里经常不是同一个（灼烧 tick 的攻击者配成
    /// <see cref="AbilityTargetting.Applier"/> 时是原始施法者，modifier 却挂在受击者身上）。
    /// 缺省让它"自动猜"的话，猜错的表现是稳定的 0，和"真的没叠层"分不出来。
    /// </summary>
    public enum StackSource
    {
        /// <summary>
        /// modifier 的挂载者（<c>modifier.Parent.GetParent&lt;Entity&gt;()</c>）。
        /// 对灼烧 / 中毒这类"挂在目标身上的 DoT"就是受击者，绝大多数情况选这个。
        /// modifier 为 null 时不可用（技能本体的 ability 级 mixin 就是 null）。
        /// </summary>
        [LabelText("Modifier挂载者")] ModifierOwner = 0,

        /// <summary>
        /// Ability 的持有者（<c>ability.Parent.GetParent&lt;Entity&gt;()</c>），
        /// 即"这套技能挂在谁身上"。读自己身上的 buff 层数（增伤、护盾）用这个。
        /// </summary>
        [LabelText("Ability持有者")] Caster = 1,

        /// <summary>
        /// 求值时传进来的 <c>entity</c>，即本次结算的攻击者。
        /// 注意它不等于"受击者"：<see cref="AbilityTargetting.Applier"/> 解析出来是施法者。
        /// </summary>
        [LabelText("攻击者")] Attacker = 2,
    }
}
