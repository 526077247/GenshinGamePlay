namespace TaoTie
{
    /// <summary>
    /// 控制生效时，对"目标正在进行中的动作"的处置方式。
    ///
    /// 这条链路以前是缺的：<c>SkillComponent</c> 只在"发起技能"时拦一道，
    /// 已经在 FSM 技能状态里的那段会继续播完、时间轴上的攻击事件照常结算伤害，
    /// 所以眩晕对"正在出招"的目标完全无效。现在由 <see cref="ControlComponent"/>
    /// 在"从无控制变为被控制"那一刻统一执行一次。
    ///
    /// 只在该控制的 <c>Deny</c> 真正让掩码从 None 变为非 None 时触发，
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
