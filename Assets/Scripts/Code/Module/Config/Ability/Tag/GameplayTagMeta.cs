using System;

namespace TaoTie
{
    /// <summary>
    /// 标记 → 行为的映射表。<b>加一种新状态只改这里（和枚举），判定代码零改动。</b>
    ///
    /// 六张表全部是"状态固有属性"，不接受逐修饰器配置 —— 否则同一个眩晕换个来源就会出现两种行为。
    /// 各表语义与「为什么必须收敛在这一处」见 <c>Book/1.AbilitySystem的战斗框架.md</c> §三：
    /// <see cref="tierOf"/> 剥夺 / <see cref="maxStackOf"/> 层数上限 / <see cref="levelOf"/> 表现等级 /
    /// <see cref="interruptOf"/> 打断 / <see cref="selectOf"/> 候选选择权 / <see cref="forcedOf"/> 强制目标。
    ///
    /// 未登记的标记按"不剥夺、不可叠、可选、无强制"处理，加标记不必同步改表。
    /// </summary>
    public static class GameplayTagMeta
    {
        /// <summary>位空间大小（<see cref="GameplayTags"/> 底层是 long，按位下标开表）。</summary>
        public const int BitCount = 64;

        #region 段掩码

        /// <summary>
        /// 段 1：控制身份位（bit 0..15）。
        /// <c>HasTag(ControlKindMask)</c> 等价于「当前是否被控」，加新控制不用改这行 ——
        /// 这是手写 <c>AnyControl</c> 清单的替代品。
        /// </summary>
        public const GameplayTags ControlKindMask = (GameplayTags)0x0000FFFFL;

        /// <summary>段 2：自身动作锁位（bit 16..23，吟唱 / 蓄力 / 僵直 / 后摇）。</summary>
        public const GameplayTags ActionLockMask = (GameplayTags)0x00FF0000L;

        /// <summary>段 3：状态标记位（bit 24..63）。</summary>
        public const GameplayTags StateFlagMask = (GameplayTags)(~0xFFFFFFL);

        /// <summary>
        /// 三个整段值的并集，只给「按整段批量表达」的字段用（目前只有 <see cref="ConfigImmuneMixin.ImmuneTags"/>），
        /// 校验时与 <see cref="KnownMask"/> 一起放行；不能出现在 <see cref="ConfigAbilityModifier.Tags"/> 上。
        /// </summary>
        public const GameplayTags GroupMask =
            GameplayTags.AllControlKind | GameplayTags.AllActionLock | GameplayTags.AllStateFlag;

        /// <summary>
        /// 段 1 + 段 2：一切"受限"状态（控制身份与动作锁）。
        /// 表现层决定要不要换受控动画、行为层决定要不要打断，判的都是它 ——
        /// 与"有没有被剥夺"不同：<see cref="GameplayTags.Slow"/> 在段 1 里但不剥夺。
        /// </summary>
        public const GameplayTags LimiterMask = ControlKindMask | ActionLockMask;

        /// <summary>
        /// 枚举里真正有名字的位。用于识别"选了枚举里根本没有的位"。
        ///
        /// 不能用段掩码拼出来：段 3 还留着 34 个空位，拼出来接近全 1，校验永远通过。
        /// 这里在静态构造里反射枚举收一次，加新标记自动生效。
        /// </summary>
        public static readonly GameplayTags KnownMask;

        /// <summary>
        /// 会打断目标当前动作的标记并集（由 <see cref="interruptOf"/> 反推）。
        /// <b>这是"打断源"的唯一判定依据</b> —— 不再是一个跟着 modifier 走的布尔，
        /// 而是标记的函数：<c>tags &amp; InterruptMask != None</c> 就等于"身上挂着打断源"。
        /// </summary>
        public static readonly GameplayTags InterruptMask;

        #endregion

        #region 能力档（剥夺集合常量）

        /// <summary>硬控档：Move|Turn|CastSkill|Attack|Sprint，即 79。</summary>
        public const ControlFlag HardStop =
            ControlFlag.Move | ControlFlag.Turn | ControlFlag.CastSkill |
            ControlFlag.Attack | ControlFlag.Sprint;

        /// <summary>定身档：Move|Turn。</summary>
        public const ControlFlag RootStop = ControlFlag.Move | ControlFlag.Turn;

        /// <summary>沉默档：CastSkill。</summary>
        public const ControlFlag CastStop = ControlFlag.CastSkill;

        /// <summary>施法锁档：Move|CastSkill（保留 Sprint，冲刺可打断）。</summary>
        public const ControlFlag MoveCastStop = ControlFlag.Move | ControlFlag.CastSkill;

        /// <summary>移动锁档：Move。</summary>
        public const ControlFlag MoveStop = ControlFlag.Move;

        /// <summary>恐惧档：Move|CastSkill|Attack（保留 Sprint：逃命）。</summary>
        public const ControlFlag FearStop =
            ControlFlag.Move | ControlFlag.CastSkill | ControlFlag.Attack;

        #endregion

        /// <summary>未登记标记的层数上限：不可叠。</summary>
        public const int DefaultMaxStack = 1;

        /// <summary>
        /// 标记 → 剥夺的能力位，下标 = 位下标（0..63）。
        /// 只登记"需要剥夺"的标记；<see cref="GameplayTags.Slow"/> 这类纯数值状态留 <see cref="ControlFlag.None"/>。
        /// </summary>
        private static readonly ControlFlag[] tierOf = new ControlFlag[BitCount];

        /// <summary>标记 → 层数上限，下标 = 位下标。只登记"允许叠层"的标记。</summary>
        private static readonly int[] maxStackOf = new int[BitCount];

        /// <summary>
        /// 标记 → 表现等级，下标 = 位下标。<b>只给表现层用</b>（决定"新来的控制值不值得换动画/图标"），
        /// 行为判定一律看 <see cref="TierOf"/>，不要拿等级做 if。
        /// 段 1 的控制身份必须登记（校验会拦），段 2 的动作锁可以留 0。
        /// </summary>
        private static readonly int[] levelOf = new int[BitCount];

        /// <summary>
        /// 标记 → 生效瞬间对"目标正在进行中的动作"的处置，下标 = 位下标。
        /// <para>
        /// <b>为什么在表里而不在配置里</b>：这回答的是"这种状态意味着什么"，
        /// 不是"这一条效果想额外做什么"。前者是状态固有、全来源一致；后者是 Action，
        /// 应该作用在 FSM 上（ForceInterrupt 之类），不该塞进 Modifier ——
        /// 塞进来就会变成"只有加状态才能打断"，连"打断但 0.1 秒后什么都不留"都表达不了。
        /// </para>
        /// <para>
        /// 只登记段 1 / 段 2。<b>段 2 的吟唱 / 蓄力 / 后摇必须留 <see cref="ControlInterrupt.None"/></b>：
        /// 它们本身就剥夺 CastSkill，若也登记成打断，吟唱开始的瞬间会把自己打断。
        /// </para>
        /// </summary>
        private static readonly ControlInterrupt[] interruptOf = new ControlInterrupt[BitCount];

        /// <summary>标记 → 候选侧选择权。<b>只登记段 3</b>：&lt;0 不可被选中（越负越硬）、0 中性、&gt;0 吸引。</summary>
        private static readonly sbyte[] selectOf = new sbyte[BitCount];

        /// <summary>标记 → 施加侧强制等级。<b>只登记段 3</b>：&gt;0 = 被迫以施加者为目标，数值即优先级。</summary>
        private static readonly byte[] forcedOf = new byte[BitCount];

        private static bool validated;

        static GameplayTagMeta()
        {
            long known = 0;
            // 按底层类型 long 取，避免 boxed long → 枚举的拆箱依赖 CLR 细节
            foreach (long raw in Enum.GetValues(typeof(GameplayTags)))
            {
                // 只收原子位：以后即使有人往枚举里加复合常量，也不会污染已知位集合
                if (raw != 0 && (raw & (raw - 1)) == 0) known |= raw;
            }
            KnownMask = (GameplayTags)known;

            for (int i = 0; i < BitCount; i++)
            {
                tierOf[i] = ControlFlag.None;
                maxStackOf[i] = DefaultMaxStack;
                levelOf[i] = 0;
                interruptOf[i] = ControlInterrupt.None;
            }

            // ── 段 1：控制身份 ──
            tierOf[Index(GameplayTags.Stun)] = HardStop;
            tierOf[Index(GameplayTags.Freeze)] = HardStop;
            tierOf[Index(GameplayTags.Petrify)] = HardStop;
            tierOf[Index(GameplayTags.Root)] = RootStop;
            tierOf[Index(GameplayTags.Silence)] = CastStop;
            tierOf[Index(GameplayTags.Fear)] = FearStop;
            // Slow 刻意不登记：它是控制身份，但不剥夺任何能力。
            // "被减速"仍然能被免疫、能被驱散、能上图标，只是 CanDo 不受影响。

            // ── 段 2：自身动作锁 ──
            tierOf[Index(GameplayTags.Chant)] = MoveCastStop;
            tierOf[Index(GameplayTags.Charge)] = MoveStop;
            tierOf[Index(GameplayTags.Stagger)] = HardStop;
            tierOf[Index(GameplayTags.Recover)] = MoveCastStop;
            tierOf[Index(GameplayTags.NoSprint)] = ControlFlag.Sprint;

            // ── 段 3：状态标记（不剥夺，只登记层数上限）──
            maxStackOf[Index(GameplayTags.Burn)] = 5;

            // ── 段 3：被选中性 / 强制目标（走各自的轴，不碰 tierOf）──
            selectOf[Index(GameplayTags.Untargetable)] = -1;
            forcedOf[Index(GameplayTags.Taunted)] = 1;
            forcedOf[Index(GameplayTags.TauntedHard)] = 2;

            // ── 表现等级（数值只是"谁盖过谁"的序，本身没有含义）──
            // 4：硬控 —— 五项能力全禁，角色彻底钉死，任何别的表现都该让位
            levelOf[Index(GameplayTags.Stun)] = 4;
            levelOf[Index(GameplayTags.Freeze)] = 4;
            levelOf[Index(GameplayTags.Petrify)] = 4;
            levelOf[Index(GameplayTags.Charm)] = 4;
            // 僵直虽在段 2（动作锁），但它是被动挨打僵住、不是自己在施法，
            // 表现上应当和硬控同级，否则"被打僵直"会被吟唱动画盖掉
            levelOf[Index(GameplayTags.Stagger)] = 4;
            // 3：恐惧 —— 仍能冲刺，但攻击和施法都被禁，接近硬控
            levelOf[Index(GameplayTags.Fear)] = 3;
            // 2：定身 / 沉默 —— 局部受限，走路或攻击至少留一样
            levelOf[Index(GameplayTags.Root)] = 2;
            levelOf[Index(GameplayTags.Silence)] = 2;
            // 1：减速 —— 不剥夺任何能力，但它是敌人施加的，该盖过自身动作锁
            levelOf[Index(GameplayTags.Slow)] = 1;
            // 0：吟唱 / 蓄力 / 后摇 —— 自己在施法，表现优先级最低，
            // 被任何控制压住都是对的（被控比施法重要）

            // ── 打断（打断了谁的动作，而不是"剥夺了谁的能力"）──
            // 段 1：敌方施加，目标失去对当前动作的控制 → 一律打断。
            // 定身是例外：只锁位移，正在放的技能不受影响（和 tierOf 不含 CastSkill 是一致的）。
            interruptOf[Index(GameplayTags.Stun)] = ControlInterrupt.CurrentSkill;
            interruptOf[Index(GameplayTags.Freeze)] = ControlInterrupt.CurrentSkill;
            interruptOf[Index(GameplayTags.Petrify)] = ControlInterrupt.CurrentSkill;
            interruptOf[Index(GameplayTags.Silence)] = ControlInterrupt.CurrentSkill;
            interruptOf[Index(GameplayTags.Fear)] = ControlInterrupt.CurrentSkill;
            // 魅惑：tierOf 没登记（不剥夺任何能力），但动作被别人接管了，必须打断。
            // 这一条就是"打断不能从剥夺推导"的证据 —— 两列各管一件事。
            interruptOf[Index(GameplayTags.Charm)] = ControlInterrupt.CurrentSkill;
            // Root / Slow 刻意不登记：定身与减速都不该打断吟唱。

            // 段 2：僵直本身就是"被打断的结果"，所以它算打断源；
            // 吟唱 / 蓄力 / 后摇 是自己的动作锁，留 None —— 否则吟唱会把自己打断。
            interruptOf[Index(GameplayTags.Stagger)] = ControlInterrupt.CurrentSkill;

            // 由表反推"打断源"掩码，避免手写第二份清单（漏一位就永久漏判）
            long interruptBits = 0;
            for (int i = 0; i < BitCount; i++)
            {
                if (interruptOf[i] != ControlInterrupt.None) interruptBits |= 1L << i;
            }
            InterruptMask = (GameplayTags)interruptBits;
        }

        /// <summary>
        /// 标记派生出的能力剥夺并集。传组合值时逐位累加 ——
        /// <c>Tags=Stun|Silence</c> 得到 <c>HardStop | CastStop</c>，与分别挂两条一致。
        /// </summary>
        public static ControlFlag TierOf(GameplayTags tags)
        {
            if (tags == GameplayTags.None) return ControlFlag.None;

            ControlFlag deny = ControlFlag.None;
            long bits = (long)tags;
            while (bits != 0)
            {
                long bit = bits & -bits;   // 最低置位
                bits ^= bit;

                deny |= tierOf[Index(bit)];
            }
            return deny;
        }

        /// <summary>
        /// 标记的层数上限。传组合值时取**最小** —— 要这组标记都能再加一层，
        /// 最紧的那个说了算，否则加完会出现"某一层的上限被冲破"。
        /// </summary>
        public static int MaxStackOf(GameplayTags tags)
        {
            if (tags == GameplayTags.None) return DefaultMaxStack;

            int result = int.MaxValue;
            long bits = (long)tags;
            while (bits != 0)
            {
                long bit = bits & -bits;
                bits ^= bit;

                int cap = maxStackOf[Index(bit)];
                if (cap < result) result = cap;
            }
            return result == int.MaxValue ? DefaultMaxStack : result;
        }

        /// <summary>
        /// 标记的表现等级。传组合值时取**最大** —— 与 <see cref="MaxStackOf"/> 取最小相反，
        /// 因为语义是"谁更强势，表现就听谁的"：同时挂着眩晕和减速，播眩晕。
        /// </summary>
        public static int LevelOf(GameplayTags tags)
        {
            if (tags == GameplayTags.None) return 0;

            int result = 0;
            long bits = (long)tags;
            while (bits != 0)
            {
                long bit = bits & -bits;
                bits ^= bit;

                int level = levelOf[Index(bit)];
                if (level > result) result = level;
            }
            return result;
        }

        /// <summary>
        /// 标记派生出的打断处置。传组合值时取**最强势的那个**（枚举值大的）——
        /// 与 <see cref="LevelOf"/> 同向：同时挂了眩晕和减速，按眩晕处理。
        /// </summary>
        public static ControlInterrupt InterruptOf(GameplayTags tags)
        {
            if (tags == GameplayTags.None) return ControlInterrupt.None;

            ControlInterrupt result = ControlInterrupt.None;
            long bits = (long)tags;
            while (bits != 0)
            {
                long bit = bits & -bits;
                bits ^= bit;

                ControlInterrupt cur = interruptOf[Index(bit)];
                if (cur > result) result = cur;
            }
            return result;
        }

        /// <summary>标记派生的候选选择权：任一排斥即排斥（取最负，安全优先），全为吸引时取最大。0 = 可选。</summary>
        public static sbyte SelectOf(GameplayTags tags)
        {
            if (tags == GameplayTags.None) return 0;

            sbyte minNeg = 0, maxPos = 0;
            long bits = (long)tags;
            while (bits != 0)
            {
                long bit = bits & -bits;
                bits ^= bit;

                sbyte v = selectOf[Index(bit)];
                if (v < minNeg) minNeg = v;
                else if (v > maxPos) maxPos = v;
            }
            return minNeg < 0 ? minNeg : maxPos;
        }

        /// <summary>标记派生的强制等级：取最大（强嘲讽压过普通嘲讽）。0 = 无强制。</summary>
        public static byte ForcedOf(GameplayTags tags)
        {
            if (tags == GameplayTags.None) return 0;

            byte result = 0;
            long bits = (long)tags;
            while (bits != 0)
            {
                long bit = bits & -bits;
                bits ^= bit;

                byte cur = forcedOf[Index(bit)];
                if (cur > result) result = cur;
            }
            return result;
        }

        /// <summary>是否是单个原子标记（只有一个位）。</summary>
        public static bool IsSingleTag(GameplayTags tag)
        {
            long v = (long)tag;
            return v != 0 && (v & (v - 1)) == 0;
        }

        /// <summary>位值 → 位下标（1 → 0、2 → 1、4 → 2 …）。只用于数组寻址，必须按 64 位算。</summary>
        public static int Index(long bit)
        {
            int index = 0;
            while ((bit >>= 1) != 0) index++;
            return index;
        }

        /// <summary>位标记 → 位下标。</summary>
        public static int Index(GameplayTags tag)
        {
            return Index((long)tag);
        }

        /// <summary>某段里置了几位。用于"一条修饰器只允许一个控制身份"这类校验。</summary>
        public static int CountBits(long value)
        {
            int count = 0;
            while (value != 0)
            {
                value &= value - 1;
                count++;
            }
            return count;
        }

        /// <summary>
        /// 加载期自检：剥夺 / 打断只能登记在段 1 / 段 2 的单个位上，层数上限至少 1。
        /// 幂等，只跑一次；表写错会在启动时就吵出来。
        /// </summary>
        public static void Validate()
        {
            if (validated) return;
            validated = true;

            for (int i = 0; i < BitCount; i++)
            {
                var tag = (GameplayTags)(1L << i);

                if (maxStackOf[i] < 1)
                {
                    Log.Error($"[GameplayTag] {tag}(bit {i}) 的层数上限 {maxStackOf[i]} 非法（至少 1）");
                }

                // 控制身份必须登记表现等级：漏登记的默认 0，会静默地永远抢不到表现，
                // 表现就是"挂了眩晕但角色还在播吟唱动画"，且不报任何错。
                //
                // 判据是"段 1 且枚举里有名字"，不是"段 1"：段 1 有 8 个预留空位（bit 8..15），
                // 它们没有名字、永远不会被配出来，要求它们登记等级只是噪音。
                // 用 KnownMask 而不是手写 0x00FF —— 以后段 1 加位会自动纳入检查。
                if ((tag & ControlKindMask) != GameplayTags.None
                    && (tag & KnownMask) != GameplayTags.None
                    && levelOf[i] < 1)
                {
                    Log.Error($"[GameplayTag] {tag}(bit {i}) 是控制身份，却没有登记表现等级 " +
                              "（levelOf），新加控制时请在 GameplayTagMeta 里补一行");
                }

                ControlFlag tier = tierOf[i];
                ControlInterrupt irq = interruptOf[i];

                // 段 3 是纯状态标记：既不剥夺能力，也不打断动作。
                // 把它登记成打断源 = 让"挂个灼烧"把别人正在放的技能打断，是最难查的那类错。
                if (irq != ControlInterrupt.None && (tag & LimiterMask) == GameplayTags.None)
                {
                    Log.Error($"[GameplayTag] {tag}(bit {i}) 不是段 1/段 2 的标记，却登记了打断 {irq}：" +
                              "段 3 是纯状态标记，禁止派生打断");
                }

                // 剥夺了 CastSkill 的段 1 控制身份却不打断，等价于"沉默期间正在放的技能还能放完"。
                // 段 2 不在此列：吟唱 / 蓄力 / 后摇 天生剥夺 CastSkill 且必须留 None。
                if ((tag & ControlKindMask) != GameplayTags.None
                    && (tier & ControlFlag.CastSkill) != ControlFlag.None
                    && irq == ControlInterrupt.None)
                {
                    Log.Error($"[GameplayTag] {tag}(bit {i}) 剥夺了 CastSkill 却没有登记打断 " +
                              "（interruptOf），正在释放的技能将无法被它打断");
                }

                // 选择轴只允许段 3，且与剥夺 / 打断互斥：同一标记不能既表示"我动不了"又表示"别人选不中我"。
                sbyte sel = selectOf[i];
                byte frc = forcedOf[i];
                if (sel != 0 || frc != 0)
                {
                    if ((tag & StateFlagMask) == GameplayTags.None)
                        Log.Error($"[GameplayTag] {tag}(bit {i}) 登记了选择轴，但不在段 3：只有状态标记能表达被选中性与强制目标");
                    if (tier != ControlFlag.None || irq != ControlInterrupt.None)
                        Log.Error($"[GameplayTag] {tag}(bit {i}) 同时登记了剥夺 / 打断与选择轴：两条轴互斥，请单独用一个段 3 标记");
                }

                if (tier == ControlFlag.None) continue;

                if ((tag & LimiterMask) == GameplayTags.None)
                {
                    Log.Error($"[GameplayTag] {tag}(bit {i}) 不是段 1/段 2 的标记，却登记了剥夺 {tier}：" +
                              "段 3 是纯状态标记，禁止派生剥夺");
                }
                if ((tier & ~ControlFlag.All) != ControlFlag.None)
                {
                    Log.Error($"[GameplayTag] {tag} 的剥夺 {tier} 含未定义的能力位");
                }
            }
        }

        /// <summary>
        /// 校验单条修饰器的标记配置。由 <see cref="ActorAbility.Create"/> 在装载时调用。
        ///
        /// 三类典型写错：
        /// 1. <c>Tags</c> 里填了枚举里不存在的位（旧版残留值、未命名的预留位）——
        ///    落库会造成重复计数 / 静默无效；
        /// 2. 一条修饰器挂了两个控制身份（Stun|Freeze）—— 同一条效果不可能既是又是，
        ///    多控制请挂多条修饰器，否则表现层和免疫判定没有唯一来源；
        /// 3. 一条修饰器挂了两个动作锁（Chant|Stagger）。
        ///
        /// 注意这里**不再校验"Tags 含能力位"**：能力位是 <see cref="ControlFlag"/> 的成员，
        /// 与 <see cref="GameplayTags"/> 已是两个独立枚举，配置面板里根本列不出来，
        /// 类型系统在编译期就挡住了 —— 校验只兜"数值对不上"这类运行期才暴露的错。
        ///
        /// 同理不再校验"Interrupt 该配什么"：它已经不在 <see cref="ConfigAbilityModifier"/> 上，
        /// 配置面没有任何入口能写错它。
        /// </summary>
        public static void ValidateModifier(ConfigAbilityModifier config, string abilityName)
        {
            if (config == null) return;

            GameplayTags tags = config.Tags;

            // 免疫校验必须放在 Tags 早退之前：纯免疫 modifier 的 Tags 就是 None，
            // 放在后面会被"没挂标记"这条短路整段跳过，等于免疫配置永不被检查。
            ValidateImmune(config, abilityName);

            // 监听 Mixin 同理：它通常挂在一条不带任何标记的功能性 modifier 上（Tags == None），
            // 也必须早于下面的早退。
            ValidateMixins(config.Mixins, abilityName, tags);

            string who = abilityName + "." + config.ModifierName;
            if (tags == GameplayTags.None) return;

            GameplayTags unknown = tags & ~KnownMask;
            if (unknown != GameplayTags.None)
            {
                Log.Error($"[GameplayTag] {who} 的 Tags 含未命名的位 {unknown}：" +
                          "预留位不能落库，请先在 GameplayTags 里给它起名；" +
                          "若是从旧版本迁移来的配置，位值可能已经变了");
            }

            int controlCount = CountBits((long)(tags & ControlKindMask));
            if (controlCount > 1)
            {
                Log.Error($"[GameplayTag] {who} 的 Tags 挂了 {controlCount} 个控制身份 {tags & ControlKindMask}：" +
                          "一条修饰器只能有一个控制身份，多控制请拆成多条修饰器");
            }

            int lockCount = CountBits((long)(tags & ActionLockMask));
            if (lockCount > 1)
            {
                Log.Error($"[GameplayTag] {who} 的 Tags 挂了 {lockCount} 个动作锁 {tags & ActionLockMask}：" +
                          "吟唱 / 蓄力 / 僵直 / 后摇 互斥，一条修饰器只能有一个");
            }
        }

        /// <summary>
        /// 校验修饰器上挂的 <see cref="ConfigImmuneMixin"/>。由 <see cref="ValidateModifier"/> 调用，
        /// <b>与 Tags 是否为空无关</b> —— 纯免疫 modifier 的 Tags 恰恰就是 None。
        ///
        /// 三类典型写错：
        /// 1. <c>ImmuneTags</c> 填了枚举里不存在的位（组掩码除外）；
        /// 2. 同一条 modifier 一边挂标记、一边免疫同一个标记（自己免疫自己后续的施加，几乎必是配错）；
        /// 3. 免疫了段 2 的动作锁 —— 吟唱 / 蓄力是自身施加的，免疫后自己永远进不去。
        ///
        /// 次数不在这里校验：<see cref="ImmuneMixin"/> 不认识次数，扣减与耗尽全在
        /// <c>Actions</c> 里用通用件配（<c>AddSpecial</c> + <c>ConfigDoActionOnSpecialsChangeMixin</c>），
        /// 代码没有「次数 key」这个入口可校验，配错表现为扣不动或移不掉，属于配置层调试。
        /// </summary>
        private static void ValidateImmune(ConfigAbilityModifier config, string abilityName)
        {
            var mixins = config.Mixins;
            if (mixins == null) return;

            string who = abilityName + "." + config.ModifierName + ".免疫";

            for (int i = 0; i < mixins.Length; i++)
            {
                var immune = mixins[i] as ConfigImmuneMixin;
                if (immune == null) continue;

                GameplayTags immuneTags = immune.ImmuneTags;
                if (immuneTags == GameplayTags.None)
                {
                    continue;
                }

                // 已知位与组掩码都放行：组掩码是"整段选择器"，见 GameplayTags.AllControlKind
                GameplayTags unknown = immuneTags & ~(KnownMask | GroupMask);
                if (unknown != GameplayTags.None)
                {
                    Log.Error($"[GameplayTag] {who} 的免疫标记含未命名的位 {unknown}：" +
                              "预留位不能落库，请先在 GameplayTags 里给它起名");
                }

                GameplayTags overlap = immuneTags & config.Tags;
                if (overlap != GameplayTags.None)
                {
                    Log.Error($"[GameplayTag] {who} 免疫 {overlap}，而这条 modifier 自己就挂着同样的标记：" +
                              "一边施加一边免疫，自身的施加会照挂上去，只会免疫后续来源，几乎肯定是配错了");
                }

                GameplayTags locks = immuneTags & ActionLockMask;
                if (locks != GameplayTags.None)
                {
                    Log.Warning($"[GameplayTag] {who} 免疫了动作锁 {locks}：" +
                                "吟唱 / 蓄力 / 后摇 都是自身施加的，免疫后自己永远进不了这些状态");
                }
            }
        }

        /// <summary>
        /// 校验一组事件监听 Mixin。由 <see cref="ValidateModifier"/>（modifier 级）和
        /// <see cref="ActorAbility.Create"/>（ability 级）各调一次 —— 这是两条独立的
        /// Mixins 数组，ability 级那条以前没有任何校验入口。
        ///
        /// <paramref name="ownerTags"/> 是宿主 modifier 自己的 Tags（ability 级传 None），
        /// 用来识别「监听的正是自己挂上的标记」这种挂载瞬间自我触发的配置。
        /// 挂在 modifier 上本身是合法的（例如"护盾期间被控则反击"），只警告自触发交集。
        /// </summary>
        public static void ValidateMixins(ConfigAbilityMixin[] mixins, string abilityName, GameplayTags ownerTags)
        {
            if (mixins == null) return;

            for (int i = 0; i < mixins.Length; i++)
            {
                var mixin = mixins[i];
                if (mixin == null) continue;

                string who = abilityName + (ownerTags == GameplayTags.None ? ".Ability" : ".Modifier");

                var tagWatch = mixin as ConfigDoActionOnEntityTagsChangeMixin;
                if (tagWatch != null)
                {
                    GameplayTags watch = tagWatch.WatchTags;

                    // 默认就是 None，漏填不会有任何报错，只会在运行时静默永不触发 —— 必须启动时吵出来
                    if (watch == GameplayTags.None)
                    {
                        Log.Error($"[GameplayTag] {who} 的标记监听没填 WatchTags：None 永远匹配不到，这条 Mixin 不会触发");
                    }
                    else
                    {
                        GameplayTags unknown = watch & ~(KnownMask | GroupMask);
                        if (unknown != GameplayTags.None)
                        {
                            Log.Error($"[GameplayTag] {who} 的 WatchTags 含未命名的位 {unknown}：" +
                                      "预留位不能落库，请先在 GameplayTags 里给它起名");
                        }

                        // 段 2 全是自身动作锁，不是「被控」；只监听它多半是把两条轴搞混了
                        if ((watch & (ControlKindMask | StateFlagMask)) == GameplayTags.None)
                        {
                            Log.Warning($"[GameplayTag] {who} 只监听了段 2 动作锁 {watch}：" +
                                        "吟唱 / 蓄力是自身施加的动作锁，不是被控；要监听控制请勾段 1 或 Tag 轴的组值");
                        }

                        GameplayTags selfHit = watch & ownerTags;
                        if (selfHit != GameplayTags.None)
                        {
                            Log.Warning($"[GameplayTag] {who} 监听的 {selfHit} 正是宿主 modifier 自己挂的标记：" +
                                        "挂载那次 Rebuild 会立刻触发 Actions；若不是故意的，请把监听移到 AbilityMixins");
                        }
                    }

                    if ((int)tagWatch.Mode < (int)TagTriggerMode.Added || (int)tagWatch.Mode > (int)TagTriggerMode.Level)
                    {
                        Log.Error($"[GameplayTag] {who} 的标记监听 Mode 越界：{(int)tagWatch.Mode}");
                    }
                    continue;
                }

                var controlWatch = mixin as ConfigDoActionOnControlStateChangeMixin;
                if (controlWatch != null)
                {
                    if ((int)controlWatch.Mode < (int)ControlTriggerMode.Begin || (int)controlWatch.Mode > (int)ControlTriggerMode.Any)
                    {
                        Log.Error($"[GameplayTag] {who} 的控制状态监听 Mode 越界：{(int)controlWatch.Mode}");
                    }

                    if (ownerTags != GameplayTags.None && TierOf(ownerTags) != ControlFlag.None)
                    {
                        Log.Warning($"[GameplayTag] {who} 挂在带剥夺位 {TierOf(ownerTags)} 的 modifier 上：" +
                                    "挂载那次 Rebuild 会立刻触发 Begin；若不是故意的，请把监听移到 AbilityMixins");
                    }
                }
            }
        }
    }
}
