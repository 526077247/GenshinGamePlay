using System;

namespace TaoTie
{
    /// <summary>
    /// 实体状态标记位。<b>这是唯一暴露给配置的标记枚举。</b>
    ///
    /// 它只回答「处于什么状态 / 因为什么受限」，<b>不回答「能做什么」</b> ——
    /// 后者是 <see cref="ControlFlag"/>，由本枚举经 <see cref="GameplayTagMeta.TierOf"/> 派生。
    ///
    /// 底层 <see cref="long"/>，64 位划三段，语义互不重叠：
    ///
    /// <b>段 1 · ControlKind（bit 0..15，16 位）控制身份位</b>：因为什么被控。
    /// 眩晕不是"另一个状态"，它就是「Move|Turn|CastSkill|Attack|Sprint 被剥夺」这件事的名字。
    /// 所以这里只登记身份（用来说表现、判免疫、做驱散），不登记剥夺 ——
    /// 剥夺由 <see cref="GameplayTagMeta.tierOf"/> 反查。一条修饰器只允许一个控制身份。
    /// 已用 8 位，余 8 位给击飞/嘲讽/束缚/睡眠/麻痹这一档。
    ///
    /// <b>段 2 · ActionLock（bit 16..23，8 位）自身动作锁</b>：吟唱 / 蓄力 / 僵直 / 后摇。
    /// 机制上与段 1 同构（同样派生剥夺），但**不算被控** ——
    /// 吟唱是自己在施法，不是被敌人控住，<c>IsControlled</c> 必须能区分这两者。
    ///
    /// <b>段 3 · StateFlag（bit 24..63，40 位）状态标记位</b>：灼烧 / 中毒 / 流血 / 易伤 / 护盾 / 冲刺。
    /// 不派生任何剥夺，只回答「是什么、几层」。层数由 <see cref="GameplayTagMeta.maxStackOf"/> 封顶。
    /// 这一段是元素 / 异常 / 增益的长期增长区，给足 40 位。
    ///
    /// <b>为什么分三段而不是平铺</b>：
    /// 1. 三段的聚合规则不同 —— 控制位取"有/无"回答"是不是"，动作锁回答"在忙什么"，
    ///    状态位取层数回答"几层"。混在一段里，同一个掩码要背三种含义，传错位不报错只会静默失效；
    /// 2. 段就是分类层级。要问"有没有被控制"不必维护一张
    ///    <c>AnyControl = Stun|Root|Silence|…</c> 的手写清单（漏加一个新控制就永久漏判），
    ///    直接用段掩码 <see cref="GameplayTagMeta.ControlKindMask"/> 一次命中，加新控制零维护；
    /// 3. 有了段就能在加载期报错（<see cref="GameplayTagMeta.ValidateModifier"/>），
    ///    把"配错"从运行时行为异常提前成一条 Error 日志。
    ///
    /// <b>位值一旦发布就不能再改</b>：它已经写进 json / bytes，改了就相当于把旧配置静默改义。
    /// 新标记一律在所属段的尾部追加；某段用尽时新开一段，不要挤进别的段。
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

        // bit 20..23 预留

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

        // bit 30..63 预留：元素异常 / 增益 / 减益的长期增长区

        #endregion

        #region 组选择器（只用于「免疫哪些标记」这类按整段批量表达的地方）

        /// <summary>
        /// 段 1 全部，<b>含 bit 8..15 的预留位</b>。
        ///
        /// 这三位不是"状态"，是给 <see cref="ConfigImmuneMixin.ImmuneTags"/> 这类字段用的
        /// 「整段选择器」：配一次就免疫全部控制身份，以后段 1 新增 Sleep / Taunt 落在预留位上时
        /// 自动被覆盖，不用回来改配置 —— 这正是手写 <c>Stun|Freeze|Root|…</c> 清单做不到的。
        ///
        /// <b>禁止用在 <see cref="ConfigAbilityModifier.Tags"/> 上</b>：那里每一位都要参与
        /// 层数聚合与「一条修饰器一个控制身份」的校验，组值没有身份，语义不成立
        /// （<see cref="GameplayTagMeta.ValidateModifier"/> 会把它们当未命名位报错）。
        ///
        /// 底层都是复合位，<see cref="GameplayTagMeta.KnownMask"/> 的静态构造只收原子位，
        /// 所以它们不会污染"已知位"集合。
        /// </summary>
        AllControlKind = 0x0000FFFF,

        /// <summary>段 2 全部（吟唱 / 蓄力 / 僵直 / 后摇）。注意免疫它等于免疫自己的动作锁。</summary>
        AllActionLock = 0x00FF0000,

        /// <summary>段 3 全部（灼烧 / 中毒 / 流血 …）。用组值免疫它请慎重：会连增益一起免疫。</summary>
        AllStateFlag = ~0xFFFFFFL,

        #endregion
    }
}
