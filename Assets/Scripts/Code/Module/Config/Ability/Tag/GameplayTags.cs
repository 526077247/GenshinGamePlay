using System;

namespace TaoTie
{
    /// <summary>
    /// 实体状态标记位。<b>唯一暴露给配置的标记枚举</b>，只回答「处于什么状态 / 因为什么受限」，
    /// 不回答「能做什么」—— 后者是 <see cref="ControlFlag"/>，由本枚举经 <see cref="GameplayTagMeta.TierOf"/> 派生。
    ///
    /// 底层 <see cref="long"/>，划三段：段 1（0..15）控制身份 / 段 2（16..23）自身动作锁 / 段 3（24..63）状态标记。
    /// 分段的理由与各段聚合规则见 <c>Book/1.AbilitySystem的战斗框架.md</c> §三。
    ///
    /// <b>位值一旦发布就不能再改</b>：已写进 json / bytes，改了等于让旧配置静默改义。新标记只在所属段尾部追加。
    /// </summary>
    [Flags]
    public enum GameplayTags : long
    {
        None = 0,

        #region 段 1 · ControlKind 控制身份位（0..15）

        /// <summary>眩晕。派生的剥夺见 <see cref="GameplayTagMeta.tierOf"/></summary>
        Stun = 1 << 0,

        /// <summary>冰冻（机制与眩晕同档，但表现 / 免疫 / 驱散分开算）</summary>
        Freeze = 1 << 1,

        /// <summary>定身：锁 Move|Turn，仍可攻击与施法</summary>
        Root = 1 << 2,

        /// <summary>沉默：锁 CastSkill，仍可移动与普攻</summary>
        Silence = 1 << 3,

        /// <summary>
        /// 减速。**是控制身份，但不剥夺任何能力**（档位为 <see cref="GameplayTags.None"/>）。
        /// 这一位正是"控制"与"剥夺"不能互相推导的证明：
        /// <c>IsControlled</c> 为真时 <c>CanDo</c> 仍然可以为真。
        /// </summary>
        Slow = 1 << 4,

        /// <summary>石化（与眩晕同档，但通常带"被击碎增伤"的额外语义）</summary>
        Petrify = 1 << 5,

        /// <summary>魅惑（同眩晕档，但目标阵营临时改变）</summary>
        Charm = 1 << 6,

        /// <summary>恐惧（锁 Move|CastSkill|Attack，保留 Sprint：逃命）</summary>
        Fear = 1 << 7,

        // bit 8..15 预留：击飞 / 嘲讽 / 束缚 / 睡眠 / 麻痹这一档

        #endregion

        #region 段 2 · ActionLock 自身动作锁（16..23）

        /// <summary>吟唱 / 读条：锁 Move|CastSkill，保留 Sprint（冲刺可打断）</summary>
        Chant = 1 << 16,

        /// <summary>蓄力：锁 Move，保留 Sprint 用于取消</summary>
        Charge = 1 << 17,

        /// <summary>僵直 / 受击硬直：锁 Move|Turn|CastSkill|Attack|Sprint</summary>
        Stagger = 1 << 18,

        /// <summary>后摇 / 收招：锁 Move|CastSkill，保留 Sprint</summary>
        Recover = 1 << 19,

        /// <summary>禁冲刺：只锁 Sprint，用于自身动作期间的区间封锁（如普攻虚化窗口）</summary>
        NoSprint = 1 << 20,

        // bit 21..23 预留

        #endregion

        #region 段 3 · StateFlag 状态标记位（24..63）

        /// <summary>灼烧：可叠层的持续伤害，默认上限 5 层（见 <see cref="GameplayTagMeta.maxStackOf"/>）</summary>
        Burn = 1L << 24,

        /// <summary>中毒</summary>
        Poison = 1L << 25,

        /// <summary>流血</summary>
        Bleed = 1L << 26,

        /// <summary>易伤：受到伤害加成</summary>
        Vulnerable = 1L << 27,

        /// <summary>拥有护盾</summary>
        Shield = 1L << 28,

        /// <summary>正在冲刺 / 突进</summary>
        Rush = 1L << 29,

        /// <summary>不可选中：选敌时被剔除（虚化 / 潜行）。选择权见 <see cref="GameplayTagMeta.SelectOf"/></summary>
        Untargetable = 1L << 30,

        /// <summary>被嘲讽：软强制攻击施加者，目标不可用才回落常规选敌</summary>
        Taunted = 1L << 31,

        /// <summary>被强嘲讽：优先级更高的软强制，压过普通嘲讽</summary>
        TauntedHard = 1L << 32,

        /// <summary>无敌：纯身份标记。伤害拦截由修饰器上的受击前钩子完成，本标记不派生任何剥夺</summary>
        Invincible = 1L << 33,

        // bit 34..63 预留：元素异常 / 增益 / 减益的长期增长区

        #endregion

        #region 组选择器（只用于「免疫哪些标记」这类按整段批量表达的地方）

        /// <summary>
        /// 段 1 全部，<b>含 bit 8..15 的预留位</b>。整段选择器，只给 <see cref="ConfigImmuneMixin.ImmuneTags"/>
        /// 这类"按整段免疫"的字段用；<b>禁止用在 <see cref="ConfigAbilityModifier.Tags"/></b> 上
        /// （组值没有身份，<see cref="GameplayTagMeta.ValidateModifier"/> 会报未命名位）。
        /// </summary>
        AllControlKind = 0x0000FFFF,

        /// <summary>段 2 全部（吟唱 / 蓄力 / 僵直 / 后摇）。注意免疫它等于免疫自己的动作锁。</summary>
        AllActionLock = 0x00FF0000,

        /// <summary>段 3 全部（灼烧 / 中毒 / 流血 …）。用组值免疫它请慎重：会连增益一起免疫。</summary>
        AllStateFlag = ~0xFFFFFFL,

        #endregion
    }
}
