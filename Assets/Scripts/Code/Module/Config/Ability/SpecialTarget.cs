namespace TaoTie
{
    /// <summary>
    /// <see cref="SetSpecial"/> / <see cref="AddSpecial"/> / <see cref="SpecialValue"/> 操作哪一层状态字典。
    ///
    /// 框架里同一个 key 可能同时存在于两层：
    /// • <see cref="ActorModifier"/> —— 每个目标 × 每次施加各一份（护盾、充能层数）；
    /// • <see cref="ActorAbility"/> —— 一个施法者一份（技能参数、常驻免疫的次数）。
    ///
    /// 所以「读写哪一层」不能靠猜，必须是显式选择；默认 <see cref="Auto"/> 是为了让
    /// 绝大多数修饰器级配置不用填这个字段。解析见
    /// <see cref="TargetHelper.ResolveSpecialTarget"/>。
    /// </summary>
    public enum SpecialTarget
    {
        /// <summary>
        /// 有修饰器就用修饰器，没有再退到 Ability。
        /// 适用于「同一条配置既能在 modifier Mixin 里跑，也能在 ability Mixin 里跑」的场合。
        /// </summary>
        Auto = 0,

        /// <summary>只认修饰器。没有修饰器上下文时报错，不静默改用 Ability。</summary>
        Modifier = 1,

        /// <summary>只认 Ability（AbilitySpecials）。用于常驻参数、跨 modifier 共享的计数。</summary>
        Ability = 2,
    }
}
