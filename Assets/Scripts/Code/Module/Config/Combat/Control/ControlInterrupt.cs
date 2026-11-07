namespace TaoTie
{
    /// <summary>
    /// 控制生效时，对"目标正在进行中的动作"的处置方式。
    ///
    /// 这条链路以前是缺的：<c>SkillComponent</c> 只在"发起技能"时拦一道，
    /// 已经在 FSM 技能状态里的那段会继续播完、时间轴上的攻击事件照常结算伤害，
    /// 所以眩晕对"正在出招"的目标完全无效。现在由 <see cref="ControlComponent"/>
    /// 在"打断源从无到有"那一刻统一执行一次。
    ///
    /// <b>这个值不在配置上</b>：它由标记派生（<see cref="GameplayTagMeta.InterruptOf"/>），
    /// 和"剥夺哪些能力"（<see cref="GameplayTagMeta.TierOf"/>）、"表现等级"（<see cref="GameplayTagMeta.LevelOf"/>）
    /// 是同一条原则 —— 同一个状态换了来源，行为必须一样。
    /// 想在某个技能上"额外打断但不留状态"，那是 Action 的事，不是 Modifier 的字段。
    ///
    /// 只在打断源（<see cref="GameplayTagMeta.InterruptMask"/>）从"无"到"有"时触发，
    /// 已经处于控制中再叠一层不会再打断（避免"被连控时反复重置动作"）。
    /// </summary>
    public enum ControlInterrupt
    {
        /// <summary>不打断。适合减速这类软控，或"允许霸体出招"的设定。</summary>
        None = 0,

        /// <summary>
        /// 打断当前技能/攻击（清 FSM 的 UseSkill）。
        /// 只作用于"正在施放的动作"；寻路不归控制层管 —— 被控时能否发起新寻路由
        /// <c>ControlFlag.Move</c> 在输入入口拦住，已在走的路径不需要中断。
        /// </summary>
        CurrentSkill = 1,
    }
}