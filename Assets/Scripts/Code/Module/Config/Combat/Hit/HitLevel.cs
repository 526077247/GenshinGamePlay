namespace TaoTie
{
    /// <summary>
    /// 击打力度等级。配置里是 int，删项会错位，所以只加不删。
    ///
    /// 当前唯一的消费点是 <see cref="CombatComponent.AfterBeAttack"/> 的 <c>&gt;= Shake</c> 门禁
    /// （决定这一下要不要触发受击抖动与击退）。<b>Light / Heavy / Air 目前与 Shake 完全等价</b>，
    /// 之所以保留：① 现有配置（如 Fox_Attack3）已经在用 2；② 后续做身体互斥冲量传递（P2）
    /// 时要按等级决定「撞到人能不能推动对方」，那才是它们真正的差异。
    /// 配了这几级却发现「没额外效果」不是 bug。
    /// </summary>
    public enum HitLevel
    {
        /// <summary>无反馈（不抖动、不击退）</summary>
        Mute = 0,
        /// <summary>轻微受击：抖动 + 击退</summary>
        Shake = 1,
        /// <summary>轻击（当前与 Shake 等价）</summary>
        Light = 2,
        /// <summary>重击（当前与 Shake 等价）</summary>
        Heavy = 3,
        /// <summary>击飞（当前与 Shake 等价；真正的「抬离地面」看 HitImpulseY &gt; 0）</summary>
        Air = 4
    }
}
