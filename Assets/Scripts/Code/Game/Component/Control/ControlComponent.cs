using System;
using System.Collections.Generic;

namespace TaoTie
{
    /// <summary>
    /// 实体状态仲裁者：实体上"能不能动 / 能不能放技能"与"处于什么状态 / 各几层"的唯一状态源。
    ///
    /// 它同时维护两条轴，分别用两个**互不相通**的枚举表达：
    /// - <b>许可轴</b> <see cref="deniedMask"/>（<see cref="ControlFlag"/>）：所有活跃修饰器的标记
    ///   派生出的剥夺并集（<see cref="GameplayTagMeta.TierOf"/>）。配置没有手写入口，
    ///   类型上也写不出来 → 出口只有 <see cref="CanDo"/>；
    /// - <b>标记轴</b> <see cref="tagMask"/> / <see cref="tagStacks"/>（<see cref="GameplayTags"/>）：
    ///   实体身上有哪些状态、各几层 → 出口 <see cref="HasTag"/> / <see cref="IsControlled"/> /
    ///   <see cref="GetTagStack"/>。
    ///
    /// 两条轴不能互相推导，这一点很容易写错：
    /// <code>
    /// 被减速     → IsControlled=true， CanDo(Move)=true    （控制身份，但不剥夺）
    /// 正在吟唱   → IsControlled=false，CanDo(Move)=false   （自身锁，不算被控）
    /// </code>
    /// 所以"被控了没"一律问 <see cref="IsControlled"/>，"能不能动"一律问 <see cref="CanDo"/>。
    ///
    /// 状态本身仍然是一个 <see cref="ActorModifier"/>（生命周期、堆叠、多来源语义全部复用，
    /// 不给 buff 另造一套计时器），但"modifier 集合 → 位掩码 / 层数"的换算只在这里做一次：
    ///
    /// <code>
    /// modifier 增删 / 叠层
    ///   → AbilityComponent 通知 NotifyModifiersChanged()
    ///   → 全量重算 deniedMask、tagMask、tagStacks（一次遍历）
    ///   → 有变化才广播 ControlStateChanged / EntityTagsChanged
    ///   → 标记里的"打断源"从无到有（GameplayTagMeta.InterruptMask）时，执行一次打断
    /// </code>
    ///
    /// 为什么全量重算而不是增量维护：加 / 删 / 叠层三条路径都要记账，任何一处漏掉就是永久性脏数据
    /// （比如眩晕位再也摘不掉，角色永远不能动）。全量重算的代价是 O(活跃 modifier 数)，
    /// 而它只在增删时跑；高频路径（每帧的移动 / 转向判定）读的是缓存掩码，O(1)。
    /// </summary>
    public class ControlComponent : Component, IComponent
    {
        /// <summary>
        /// 位空间大小。<see cref="GameplayTags"/> 的底层类型是 long，
        /// 所以层数数组按 0..63 的位下标开，而不是按"第几个标记"开 —— 这样加新标记不用改数组大小。
        /// </summary>
        private const int TagBitCount = GameplayTagMeta.BitCount;

        private AbilityComponent abilityComponent;

        /// <summary>
        /// 免疫源注册表（<see cref="ImmuneMixin"/> 在宿主 afterAdd 里注册、beforeRemove 里注销）。
        ///
        /// 只放一张 0~2 项的小表，不做全量重算：注册跟宿主生命周期走，
        /// 与 <see cref="Rebuild"/> 聚合标记/层数是两件互不干扰的事。
        /// 不额外维护免疫掩码 —— 源通常 0~2 条，直接遍历即可，
        /// 多一份掩码就要同步注册/注销/销毁三处，反而多一个一致性风险。
        /// </summary>
        private List<ImmuneMixin> immuneSources;

        /// <summary>当前被剥夺的能力位并集（<see cref="ControlFlag"/>）。全部活跃来源的或。</summary>
        private ControlFlag deniedMask;
        /// <summary>
        /// 当前最高控制等级，只给表现层用（决定"新来的控制值不值得换表现"），不参与判定。
        /// 0 = 没有任何受限标记，或最低优先级的自身动作锁（吟唱 / 蓄力 / 后摇）。
        /// </summary>
        private int topLevel;

        /// <summary>当前挂着的状态标记并集（域 B）。</summary>
        private GameplayTags tagMask;
        /// <summary>每个标记的层数总和，下标 = 位下标（0..63）。</summary>
        private int[] tagStacks;
        /// <summary>重算缓冲：先算到这份里，再和当前值比对，用于判断"层数变了但标记集合没变"。</summary>
        private int[] tagStacksNext;

        /// <summary>当前被剥夺的能力位并集，O(1) 读取。</summary>
        public ControlFlag DeniedMask => deniedMask;

        /// <summary>当前挂着的状态标记并集，O(1) 读取。</summary>
        public GameplayTags Tags => tagMask;

        /// <summary>
        /// 当前最高控制等级（数值来自 <see cref="GameplayTagMeta.LevelOf"/>，与配置无关）。
        /// 表现层比较"新来的控制值不值得换动画"时用，
        /// 行为判定不要看它 —— 一律看 <see cref="DeniedMask"/>。
        /// 0 表示没有任何受限标记，或只有最低优先级的自身动作锁（吟唱 / 蓄力 / 后摇）。
        /// </summary>
        public int TopControlLevel => topLevel;

        /// <summary>
        /// 能力位掩码变化回调（同实体内部组件用，省掉走消息系统的一次查找）。
        /// 跨系统（UI / 模型）请订阅 <see cref="MessageId.ControlStateChanged"/>。
        /// </summary>
        public event Action<ControlFlag, ControlFlag> OnMaskChanged;

        /// <summary>
        /// 标记变化回调：old、new、以及"是否只是层数变了（标记集合没变）"。
        /// 跨系统请订阅 <see cref="MessageId.EntityTagsChanged"/>。
        /// </summary>
        public event Action<GameplayTags, GameplayTags, bool> OnTagsChanged;

        #region IComponent

        public void Init()
        {
            // 幂等自检：登记表写错（比如把状态位登记成"剥夺"）在这里就吵出来，而不是等行为诡异
            GameplayTagMeta.Validate();

            deniedMask = ControlFlag.None;
            topLevel = 0;
            tagMask = GameplayTags.None;
            tagStacks = null;
            tagStacksNext = null;
            OnMaskChanged = null;
            OnTagsChanged = null;

            // 免疫表跟着组件走：Component 从对象池 Fetch，不清会在复用后残留上一个实体的免疫源，
            // 表现为"新实体天生免控"。
            immuneSources?.Clear();
            immuneSources = null;

            // 初始静默重建：实体创建时可能已经带着控制类 modifier（如出生自带 buff），
            // 但那属于"已有状态"，不该触发打断。
            Rebuild(true);
        }

        public void Destroy()
        {
            OnMaskChanged = null;
            OnTagsChanged = null;
            abilityComponent = null;
            deniedMask = ControlFlag.None;
            tagMask = GameplayTags.None;
            tagStacks = null;
            tagStacksNext = null;
            topLevel = 0;
            immuneSources?.Clear();
            immuneSources = null;
        }

        #endregion

        #region 许可轴

        /// <summary>
        /// 是否还能执行某项行为。所有行为入口的唯一判定出口。
        /// 参数是 <see cref="ControlFlag"/>（不是 <see cref="GameplayTags"/>）：
        /// 状态标记要在剥夺之外表达"我禁了移动"，现在类型上就不成立。
        /// 传单个能力位；需要多个时请分别判断 —— 传组合位会变成"全都被禁"才算禁。
        /// </summary>
        public bool CanDo(ControlFlag flag)
        {
            return flag == ControlFlag.None || (deniedMask & flag) == 0;
        }

        /// <summary>
        /// 是否正被控制（段 1 有任一位）。
        ///
        /// <b>不要用 <c>DeniedMask != None</c> 代替它</b>：正在吟唱时能力被锁但没被控，
        /// 被减速时被控但能力没被锁。两者不互推。
        ///
        /// 加新控制标记不需要改这里 —— 段掩码自动覆盖，这也是它相对手写
        /// <c>AnyControl = Stun|Root|…</c> 清单的意义（漏加一个就永久漏判）。
        /// </summary>
        public bool IsControlled => (tagMask & GameplayTagMeta.ControlKindMask) != GameplayTags.None;

        /// <summary>
        /// 是否被完全钉死（<see cref="GameplayTagMeta.HardStop"/> 五项能力全部被剥夺）。
        ///
        /// 用"全禁"而不是"命中任一位"：沉默只禁 CastSkill、定身只禁 Move|Turn，
        /// 拿"命中任一位"去判会把它们也算成硬控，表现层就会对一个还能走路的角色播倒地动画。
        /// </summary>
        public bool IsHardControlled => (deniedMask & GameplayTagMeta.HardStop) == GameplayTagMeta.HardStop;

        #endregion

        #region 标记轴

        /// <summary>
        /// 是否挂着指定标记。传组合值表示"命中任一" ——
        /// <c>HasTag(GameplayTagMeta.ControlKindMask)</c> 一次覆盖全部控制身份，
        /// 以后新增控制标记不需要回来改这行（段掩码自动收进去）。
        /// </summary>
        public bool HasTag(GameplayTags tag)
        {
            return tag != GameplayTags.None && (tagMask & tag) != GameplayTags.None;
        }

        /// <summary>是否同时挂着全部指定标记。</summary>
        public bool HasAllTags(GameplayTags tags)
        {
            return tags != GameplayTags.None && (tagMask & tags) == tags;
        }

        /// <summary>
        /// 某个标记当前的层数总和。
        ///
        /// 只能传单个原子标记：组合值（如 <c>ControlKindMask</c>）没有"层"的概念，
        /// 取和还是取最大都是拍脑袋，所以直接报错，逼调用方写清楚要问哪个。
        /// </summary>
        public int GetTagStack(GameplayTags tag)
        {
            if (tag == GameplayTags.None) return 0;
            if (!GameplayTagMeta.IsSingleTag(tag))
            {
                Log.Error($"[GameplayTags] GetTagStack 只接受单个标记，收到组合值 {tag}({(long)tag})");
                return 0;
            }
            if (tagStacks == null) return 0;

            int index = GameplayTagMeta.Index((long)tag);
            return index < TagBitCount ? tagStacks[index] : 0;
        }

        /// <summary>某个标记的层数上限（来自 <see cref="GameplayTagMeta"/>；组合值取最小）。</summary>
        public int GetTagMaxStack(GameplayTags tags)
        {
            return GameplayTagMeta.MaxStackOf(tags);
        }

        /// <summary>
        /// 这组标记还能再加几层（取组内最小余量）。
        ///
        /// 多位时是**原子**的：要这组标记都能再加一层才算有余量，
        /// 否则会出现"Stun 加上了、Vulnerable 没加上"这种半生效状态，
        /// 回退时还要按标记逐个记账。
        /// </summary>
        public int GetTagRoom(GameplayTags tags)
        {
            if (tags == GameplayTags.None) return int.MaxValue;

            int room = int.MaxValue;
            long bits = (long)tags;
            while (bits != 0)
            {
                long bit = bits & -bits;   // 最低置位
                bits ^= bit;

                var tag = (GameplayTags)bit;
                int left = GameplayTagMeta.MaxStackOf(tag) - GetTagStack(tag);
                if (left < room) room = left;
            }

            if (room == int.MaxValue) return int.MaxValue;
            return room > 0 ? room : 0;
        }

        /// <summary>
        /// 清掉挂着指定标记的所有 modifier（净化 / 驱散）。返回清掉的条数。
        /// </summary>
        public int ClearTags(GameplayTags tags)
        {
            if (IsDispose || tags == GameplayTags.None) return 0;
            if (abilityComponent == null || abilityComponent.IsDispose)
                abilityComponent = GetComponent<AbilityComponent>();

            return abilityComponent?.RemoveModifiersByTag(tags) ?? 0;
        }

        #endregion

        #region 免疫

        /// <summary>由 <see cref="ImmuneMixin"/> 在宿主 afterAdd 时调用。</summary>
        public void RegisterImmune(ImmuneMixin immune)
        {
            if (IsDispose || immune == null) return;
            (immuneSources ??= new List<ImmuneMixin>(2)).Add(immune);
        }

        /// <summary>
        /// 由 <see cref="ImmuneMixin"/> 在宿主 beforeRemove 时调用。
        /// 实体销毁时组件 Destroy 顺序不保证：表可能已经清空，重复注销同样无害。
        /// </summary>
        public void UnregisterImmune(ImmuneMixin immune)
        {
            immuneSources?.Remove(immune);
        }

        /// <summary>
        /// 查有没有能免疫这组标记的来源。命中**任一位**即算命中 ——
        /// 免疫是「拒绝」语义，只要有一条免疫覆盖了其中一个标记，整条施加就不该发生。
        /// 调用点只有 <see cref="AbilityComponent.ApplyModifier"/>，且必须在堆叠分支之前。
        ///
        /// <b>优先返回不限次的源</b>：次数是资源，不该被冗余的永久免疫白白吃掉。
        /// 身上同时有「常驻免控」和「免疫 3 次」时，前者能挡就不消耗后者。
        /// 没有不限次源时返回第一个有限次源（注册顺序 = 挂载顺序，可预测）。
        /// <see cref="ImmuneMixin.CanImmune"/> 自带 Config 非空保护，
        /// 已 Dispose 回池的实例不会误命中。
        /// </summary>
        public bool TryGetImmune(GameplayTags tags, out ImmuneMixin source)
        {
            source = null;
            if (IsDispose || tags == GameplayTags.None || immuneSources == null || immuneSources.Count == 0)
                return false;

            for (int i = 0; i < immuneSources.Count; i++)
            {
                var immune = immuneSources[i];
                if (immune == null || !immune.CanImmune(tags)) continue;

                if (immune.Unlimited)
                {
                    source = immune;
                    return true;
                }

                source ??= immune;
            }

            return source != null;
        }

        #endregion

        /// <summary>
        /// modifier 增删后由 <see cref="AbilityComponent"/> 调用。
        /// 只在此时做全量重算，避免每帧扫描。
        /// </summary>
        public void NotifyModifiersChanged()
        {
            if (IsDispose) return;
            Rebuild(false);
        }

        private void Rebuild(bool silent)
        {
            if (abilityComponent == null || abilityComponent.IsDispose)
                abilityComponent = GetComponent<AbilityComponent>();

            EnsureTagArrays();
            Array.Clear(tagStacksNext, 0, TagBitCount);

            ControlFlag deny = ControlFlag.None;
            GameplayTags tags = GameplayTags.None;
            int bestLevel = 0;

            var mods = abilityComponent?.Modifiers;
            if (mods != null)
            {
                for (var node = mods.First; node != null; node = node.Next)
                {
                    var modifier = node.Value;
                    if (modifier == null || modifier.IsDispose || modifier.Config == null) continue;

                    var config = modifier.Config;

                    // 标记先记：它既可能派生剥夺，也要累加层数
                    GameplayTags modTags = config.Tags;
                    if (modTags != GameplayTags.None)
                    {
                        tags |= modTags;
                        AccumulateStack(modTags, modifier.StackCount);
                    }

                    // 剥夺完全由标记派生（Stun → HardStop、Chant → Move|CastSkill …）。
                    // 配置没有手写剥夺的入口，所以不可能出现"挂了眩晕却还动得了"的脱钩：
                    // 身份与剥夺是同一份数据推导出来的两个视图。
                    deny |= GameplayTagMeta.TierOf(modTags);

                    // 等级也只看标记，不看"有没有剥夺" —— 减速不剥夺任何能力，
                    // 但它的等级同样要参与比较，否则"减速压过吟唱"这种表现规则表达不出来。
                    // 段 3 的标记一样会走到这里（等级默认 0），以后给灼烧登记等级就直接生效。
                    int level = GameplayTagMeta.LevelOf(modTags);
                    if (level > bestLevel) bestLevel = level;
                }
            }

            // 层数：先比再交换 —— 否则"只叠了一层、标记集合没变"会被判成"什么都没变"
            bool stackChanged = !StackEquals();
            (tagStacks, tagStacksNext) = (tagStacksNext, tagStacks);

            GameplayTags oldTags = tagMask;
            tagMask = tags;
            bool tagsChanged = oldTags != tags;

            ControlFlag old = deniedMask;

            // ── 两条轴的结果都在广播之前落盘 ──
            //
            // 订阅方（DoActionOnEntityTagsChangeMixin 的 Actions、UI 头顶图标）在回调里
            // 读 CanDo / DeniedMask / TopControlLevel 必须拿到新值：以前 Tag 广播排在
            // deniedMask 赋值之前，眩晕刚挂上时 Actions 里问 CanDo(Move) 会得到 true。
            //
            // 还有一层重入原因：Actions 里 AttachModifier 会嵌套触发一次 Rebuild。
            // 若那时 deniedMask / topLevel 还留着旧值，嵌套重算会把同一个变化再广播一遍。
            // 提前赋值后嵌套 Rebuild 看到的 old 已经是新值，自然不再重复广播。
            deniedMask = deny;
            topLevel = bestLevel;
            bool maskChanged = old != deny;

            // 打断判定独立于掩码：吟唱这类"自身就剥夺 CastSkill"的状态下，掩码早就是非 0 了，
            // 若仍按"掩码 0→非 0"判断，眩晕永远打不断吟唱。
            //
            // 改成看"打断源从无到有"，而打断源本身就是标记的函数（InterruptMask）——
            // 吟唱 / 蓄力 / 后摇 不在 InterruptMask 里，眩晕 / 僵直 / 魅惑在，
            // 所以只在真正来控的那一次打断，且不需要第三个状态变量来记"上次有没有"。
            GameplayTags oldInterrupt = oldTags & GameplayTagMeta.InterruptMask;
            GameplayTags newInterrupt = tags & GameplayTagMeta.InterruptMask;
            if (!silent && oldInterrupt == GameplayTags.None && newInterrupt != GameplayTags.None)
            {
                ApplyInterrupt(GameplayTagMeta.InterruptOf(newInterrupt));
            }

            if (tagsChanged || stackChanged)
            {
                OnTagsChanged?.Invoke(oldTags, tags, stackChanged);

                if (Messager.Instance != null)
                {
                    Messager.Instance.Broadcast(Id, MessageId.EntityTagsChanged, new EntityTagsEvent
                    {
                        Id = Id,
                        OldTags = oldTags,
                        NewTags = tags,
                        StackChanged = stackChanged,
                    });
                }
            }

            // 掩码没变就到此为止：等级变化、以及"同一种控制换了个来源"都不单独广播，
            // 免得 UI 每次命中都重建一次图标。
            // topLevel / deniedMask 已在广播之前刷新过，这里 return 不会再留下脏值 ——
            // 只挂减速时掩码恒为 None，旧实现把 topLevel 放在 return 之前才不出错，
            // 现在两块一起提前，这个约束就不存在了。
            if (!maskChanged) return;

            OnMaskChanged?.Invoke(old, deny);

            if (Messager.Instance != null)
            {
                Messager.Instance.Broadcast(Id, MessageId.ControlStateChanged, new ControlStateEvent
                {
                    Id = Id,
                    OldMask = old,
                    NewMask = deny,
                    Level = bestLevel,
                });
            }
        }

        /// <summary>
        /// 把一组标记的层数累加进重算缓冲。
        /// 一个 modifier 挂多个标记时逐个位累加 —— 复合值不会出现（加载期校验拦住），
        /// 所以这里是"原子标记 → 层数"的一一对应。
        /// </summary>
        private void AccumulateStack(GameplayTags tags, int stack)
        {
            if (stack <= 0) return;

            long bits = (long)tags;
            while (bits != 0)
            {
                long bit = bits & -bits;
                bits ^= bit;

                int index = GameplayTagMeta.Index(bit);
                if (index < TagBitCount) tagStacksNext[index] += stack;
            }
        }

        private bool StackEquals()
        {
            for (int i = 0; i < TagBitCount; i++)
            {
                if (tagStacks[i] != tagStacksNext[i]) return false;
            }
            return true;
        }

        /// <summary>
        /// 按需分配层数数组。只在首次重算时分配，之后两块缓冲交替复用（零 GC）。
        /// </summary>
        private void EnsureTagArrays()
        {
            if (tagStacks != null && tagStacksNext != null) return;

            tagStacks = new int[TagBitCount];
            tagStacksNext = new int[TagBitCount];
        }

        /// <summary>
        /// 控制生效瞬间的处置：打断当前动作。
        /// 这里是"打断"的唯一落点 —— 以前 <see cref="CombatComponent.ReleaseSkillImmediately"/>
        /// 只有 AI 的失败分支会调，控制链路从来不调，导致眩晕打不断正在施放的技能。
        ///
        /// 只处理技能 / 动作，不碰寻路：大世界的六边形寻路由输入层自己管，
        /// 被控时"能不能发起新寻路"由 <see cref="ControlFlag.Move"/> 在入口拦住，
        /// 已经在走的路径不需要控制层介入。
        /// </summary>
        private void ApplyInterrupt(ControlInterrupt interrupt)
        {
            if (interrupt == ControlInterrupt.None) return;

            GetComponent<CombatComponent>()?.ReleaseSkillImmediately();
        }
    }
}
