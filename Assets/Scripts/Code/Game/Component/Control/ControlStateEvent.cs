namespace TaoTie
{
    /// <summary>
    /// 控制状态变化广播载荷（<see cref="MessageId.ControlStateChanged"/>）。
    ///
    /// 广播只在"掩码真的变了"时发一次，所以订阅方不需要轮询：
    /// UI 头顶图标、模型层眩晕特效、AI 决策都可以靠它驱动。
    ///
    /// <see cref="Level"/> 是当前生效控制里的最高等级，供表现层判断"新来的控制值不值得换表现"；
    /// 行为判定请用 <see cref="NewMask"/>，不要用等级做 if。
    ///
    /// 掩码类型是 <see cref="ControlFlag"/> 而非 <see cref="GameplayTags"/>：
    /// 这里传的是"能力被剥夺成什么样"的结论，想知道"因为什么被控"请订阅
    /// <see cref="MessageId.EntityTagsChanged"/>。两者刻意不合并成一个字段 ——
    /// 一个实体可以同时被眩晕和灼烧，但"能不能动"只有一个答案。
    /// </summary>
    public struct ControlStateEvent
    {
        public long Id;
        /// <summary>变化前的能力剥夺掩码</summary>
        public ControlFlag OldMask;
        /// <summary>变化后的能力剥夺掩码（None = 已完全解除控制）</summary>
        public ControlFlag NewMask;
        /// <summary>当前生效控制中的最高表现等级（来自 GameplayTagMeta.LevelOf，不是配置）</summary>
        public int Level;

        /// <summary>
        /// 是否从"无控制"进入"被控制"。
        /// <b>不要拿它当「控制刚附加」用</b>：吟唱 / 蓄力已经剥夺了部分能力，
        /// 此时再被眩晕，OldMask 非 None，这里为 false 但控制确实刚加上。
        /// 需要「新增剥夺」语义请用 <see cref="Gained"/>。
        /// </summary>
        public bool IsBegin => OldMask == ControlFlag.None && NewMask != ControlFlag.None;
        /// <summary>是否已完全解除控制</summary>
        public bool IsEnd => NewMask == ControlFlag.None;

        /// <summary>
        /// 本次<b>新获得</b>的能力剥夺位（<c>NewMask &amp; ~OldMask</c>）。
        /// 与 <see cref="EntityTagsEvent.Added"/> 对称：判定「控制刚附加」一律看它，
        /// 而不是 <see cref="IsBegin"/> —— 后者只认「从全无到有」这一种情况。
        /// </summary>
        public ControlFlag Gained => NewMask & ~OldMask;
        /// <summary>本次<b>恢复</b>的能力位（<c>OldMask &amp; ~NewMask</c>）。</summary>
        public ControlFlag Lost => OldMask & ~NewMask;
    }

    /// <summary>
    /// 状态标记变化广播载荷（<see cref="MessageId.EntityTagsChanged"/>）。
    ///
    /// 只广播"哪些标记 / 层数变了"，<b>不带全量层数</b>：一次命中就 +1 层，
    /// 把层数塞进事件会让 UI 每次命中都重建一次。订阅方拿到变化后自己
    /// <see cref="ControlComponent.GetTagStack"/> 拉需要的那个值。
    ///
    /// <see cref="StackChanged"/> 为 true 而 <see cref="OldTags"/> == <see cref="NewTags"/> 时，
    /// 表示"标记集合没变、只是层数涨落"（例如灼烧从 2 层叠到 3 层）。
    /// </summary>
    public struct EntityTagsEvent
    {
        public long Id;
        /// <summary>变化前的标记集合</summary>
        public GameplayTags OldTags;
        /// <summary>变化后的标记集合（None = 已经没有任何标记）</summary>
        public GameplayTags NewTags;
        /// <summary>这次是否伴随层数变化</summary>
        public bool StackChanged;

        /// <summary>这次新挂上的标记（<c>NewTags &amp; ~OldTags</c>）</summary>
        public GameplayTags Added => NewTags & ~OldTags;
        /// <summary>这次摘掉的标记（<c>OldTags &amp; ~NewTags</c>）</summary>
        public GameplayTags Removed => OldTags & ~NewTags;
    }
}
