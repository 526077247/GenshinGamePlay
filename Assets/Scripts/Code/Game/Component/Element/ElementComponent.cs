using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 元素附着容器：<b>按元素分槽存元素量，并保证"身上不会同时存在两个能反应的元素"</b>。
    ///
    /// 三条边界，界内是本类的全部职责，界外一律不碰：
    /// 1. <b>内建反应判定，不内建反应效果</b>：每次附着后自动查反应表，命中就<b>等量抵消</b>双方元素量
    ///    （各扣 <c>min(两边的量)</c>）并广播 <see cref="MessageId.ElementReaction"/> ——
    ///    判定和抵消在这里，<b>伤害 / 增伤 / buff 一律不算</b>，
    ///    由配置层挂 <see cref="ConfigElementReactionMixin"/> 的 Ability 自己算（数值用 <see cref="ElementReactionValue"/> 读）。
    ///    本类<b>不提供任何伤害倍率</b>：元素与伤害的关系只在配置层表达。
    /// 2. <b>不进 GameplayTags</b>：元素量与 <see cref="ControlComponent"/> 的标记池是两套账。
    ///    元素会衰减、会被按量消耗，标记是计数器 —— 混进一个 64 位枚举会丢掉"量"这个维度。
    /// 3. <b>不认识任何具体元素</b>：有几种、叫什么、上限多少、谁和谁能反应，
    ///    全部来自 <see cref="ElementConfigCategory"/> 与 <see cref="ElementReactionCategory"/>（Excel 表）。本类只认 Id。
    ///
    /// <b>元素量是整数，不记小数</b>：附着、衰减、消耗全部向下取整，读出来永远是整数。
    /// 所以配置里可以放心写 0.5/秒 的衰减速率，但不要指望"挂 2.7 点"这种事 —— 那会变成 2 点。
    ///
    /// 时序约定（改 <see cref="AttackHelper.DamageClose"/> 时必须守住）：
    /// 命中结算<b>先让配置层的受击钩子读旧元素量</b>（BeforeBeAttack / AfterBeAttack 里读到的都是"这一击之前"的状态），
    /// <b>整击结算完才附着新元素</b>。否则同一次攻击打出的元素会立刻被自己触发的反应吃掉。
    ///
    /// 附着有<b>两条入口</b>，分别对应伤害型和非伤害型效果，别混用：
    /// • 伤害型：<see cref="AttackHelper.DamageClose"/> 末尾按
    ///   <see cref="ConfigAttackProperty.Element"/> / <c>ElementDurability</c> 附着；
    /// • 非伤害型：<see cref="ActorModifier"/> 挂到实体上时按
    ///   <see cref="ConfigAbilityModifier.Element"/> / <c>ElementDurability</c> 附着 ——
    ///   给队友挂 buff、给自己上状态这类没有伤害结算的效果靠这条。
    /// 同一个元素在两条上都配会挂两次。
    /// </summary>
    public class ElementComponent : Component, IComponent, IUpdate
    {
        private struct Slot
        {
            /// <summary>
            /// 当前元素量，<b>整数，不记小数</b>。&lt;= 0 即该元素没挂上。
            /// 所有写入路径（附着 / 衰减 / 消耗）都向下取整，读出来就是整数。
            /// </summary>
            public int Durability;

            /// <summary>
            /// 衰减不足 1 点的余量。衰减速率是小数（如 0.5/秒），直接往整数上减会每帧丢 1 点，
            /// 所以先在这里攒够 1 才真正扣 1 —— 0.5/秒 就是每 2 秒掉 1 点。
            /// </summary>
            public float DecayAcc;

            /// <summary>最近一次附着的来源实体 Id，供配置层判断"这是我打的火吗"。</summary>
            public long ApplierId;
        }

        /// <summary>
        /// 下标 = 元素 Id，长度 = 可附着元素的最大 Id + 1。
        /// <b>惰性创建</b>：实体创建时配置可能还没加载完，那时拿不到表长度；
        /// 建之后不再重算，所以元素表增删 Id 要重进游戏。
        /// </summary>
        private Slot[] slots;

        /// <summary>已挂上的元素个数。<b>唯一用途是让 Update 在完全没附着时零成本早退</b>，
        /// 不要拿它当"挂了哪些元素"用，那要看 <see cref="GetAttachedMask"/>。</summary>
        private int activeCount;

        /// <summary>位图缓存，maskDirty 时由 <see cref="FlushMask"/> 重算。</summary>
        private long mask;
        private bool maskDirty;

        /// <summary>元素表里没有可附着元素，只吵一次（每次命中都打会刷屏）。</summary>
        private static bool warnedNoTable;

        /// <summary>非法元素 Id 只吵一次，避免废配置在每次命中上刷日志。</summary>
        private static readonly HashSet<int> warnedIds = new HashSet<int>();

        /// <summary>最近一次反应的 Id（0 = 这个实体还没发生过反应）。</summary>
        public int LastReactionId { get; private set; }

        /// <summary>最近一次反应的触发元素（刚挂上的那个）。</summary>
        public int LastReactionTrigger { get; private set; }

        /// <summary>最近一次反应撞上的已挂元素。</summary>
        public int LastReactionOther { get; private set; }

        /// <summary>最近一次反应实际扣掉的触发元素量。</summary>
        public int LastReactionTriggerConsumed { get; private set; }

        /// <summary>最近一次反应实际扣掉的对方元素量。</summary>
        public int LastReactionOtherConsumed { get; private set; }

        #region IComponent

        public void Init()
        {
            slots = null;
            activeCount = 0;
            mask = 0;
            maskDirty = false;
        }

        public void Destroy()
        {
            slots = null;
            activeCount = 0;
            mask = 0;
            maskDirty = false;
        }

        #endregion

        #region 衰减

        /// <summary>
        /// 自然衰减。<b>用的是被 timeScale 缩放过的帧间隔</b>（<see cref="GameTimerManager.GetDeltaTime"/>），
        /// 所以时停 / 慢动作期间元素不会掉 —— 与"时停里子弹也不动"同一口径。
        ///
        /// 元素量是整数，所以衰减速率的小数部分不直接减，先攒在 <see cref="Slot.DecayAcc"/> 里，
        /// 攒够 1 点才扣 1 点：0.5/秒 = 每 2 秒掉 1 点，而不是每帧掉 1 点。
        ///
        /// 只处理 <see cref="ElementConfig.DecayPerSecond"/> &gt; 0 的元素；
        /// 填 0 的元素永远留在这里，直到被 <see cref="ConsumeElement"/> 消耗或被 <see cref="Clear"/> 清掉。
        /// </summary>
        public void Update()
        {
            if (activeCount == 0) return;

            long deltaMs = GameTimerManager.Instance.GetDeltaTime();
            if (deltaMs <= 0) return;
            float delta = deltaMs * 0.001f;

            for (int i = 1; i < slots.Length; i++)
            {
                if (slots[i].Durability <= 0) continue;
                var element = GetConfig(i);
                if (element == null || element.DecayPerSecond <= 0) continue;

                slots[i].DecayAcc += element.DecayPerSecond * delta;
                if (slots[i].DecayAcc < 1f) continue;

                int whole = (int)slots[i].DecayAcc; // 正数截断 = 向下取整
                slots[i].DecayAcc -= whole;
                slots[i].Durability -= whole;
                if (slots[i].Durability > 0) continue;

                slots[i].Durability = 0;
                slots[i].DecayAcc = 0;
                slots[i].ApplierId = 0;
                activeCount--;
                maskDirty = true;
            }

            FlushMask();
        }

        #endregion

        #region 附着

        /// <summary>
        /// 附着元素：<b>叠加而不是覆盖</b>，并按 <see cref="ElementConfig.MaxDurability"/> 封顶。
        /// 来源 Id 总是刷新为最新施加者（"谁最后点的火"），旧来源不留。
        ///
        /// <b>元素量是整数，入参一律向下取整</b>：传 2.9 就是挂 2 点，传 0.9 就是一点不挂。
        ///
        /// 伤害型（<see cref="ConfigAttackProperty.Element"/>）和非伤害型（<see cref="ConfigAbilityModifier.Element"/>）
        /// 两条入口共用本方法，都只是把「元素 Id + 元素量」交进来。
        /// </summary>
        /// <returns>实际入账的元素量之和；0 表示一点没挂上（元素非法 / 上限为 0 / 已满）。</returns>
        public float Attach(int elementId, float durability, long applierId)
        {
            int total = Mathf.FloorToInt(durability);
            if (total <= 0) return 0;

            if (GetConfig(elementId) == null)
            {
                WarnOnce(elementId, $"元素 Id {elementId} 不在元素表里，本次附着被丢弃");
                return 0;
            }

            return AttachOne(elementId, total, applierId);
        }

        /// <summary>
        /// 挂一个元素，挂完<b>立刻结算它引发的反应</b>。
        /// </summary>
        private int AttachOne(int elementId, int durability, long applierId)
        {
            int applied = AttachSingle(elementId, durability, applierId);
            if (applied > 0) ResolveReaction(elementId);
            return applied;
        }

        /// <summary>
        /// 往单个槽里入账。元素量按上限封顶后仍是整数。
        /// </summary>
        private int AttachSingle(int elementId, int durability, long applierId)
        {
            if (durability <= 0) return 0;
            if (!EnsureSlots()) return 0;

            var element = GetConfig(elementId);
            if (element == null)
            {
                WarnOnce(elementId, $"元素 Id {elementId} 不在元素表里，本次附着被丢弃");
                return 0;
            }

            // 上限在表里就是 int（元素量不记小数），这里不再二次取整
            int cap = element.MaxDurability;
            if (cap <= 0) return 0;                   // 未配上限 = 不可附着，这是配置常态，不是错误
            if (elementId >= slots.Length) return 0;  // GetMaxAttachId 已覆盖所有可附着行，走到这里说明表被改过

            bool wasEmpty = slots[elementId].Durability <= 0;
            int next = slots[elementId].Durability + durability;
            if (next > cap) next = cap;

            int applied = next - slots[elementId].Durability;
            if (applied <= 0) return 0;

            slots[elementId].Durability = next;
            slots[elementId].ApplierId = applierId;
            if (wasEmpty)
            {
                activeCount++;
                maskDirty = true;
                FlushMask();
            }
            return applied;
        }

        #endregion

        #region 元素反应

        /// <summary>
        /// 结算刚挂上的这个元素引发的反应，<b>保证挂完之后身上不存在"能反应却共存"的元素对</b>。
        ///
        /// 规则只有一条：<b>双方按数值等量抵消</b>，各扣 <c>min(两边的量)</c>。
        /// 例：挂 3 点水打在身上已有的 1 点火上 → 水扣 1 剩 2，火清零。
        /// 量少的一方必然被扣光，所以"不会共存"是这条规则的自然结果，不需要额外兜底。
        ///
        /// 谁和谁能反应全部来自 <see cref="ElementReactionCategory"/>（Excel 反应表）。
        /// 本类<b>只负责判定 + 抵消 + 广播</b>，反应产生什么效果（伤害 / 增伤 / 上 buff）一律交给
        /// 配置层挂的元素反应 Ability（<see cref="ConfigElementReactionMixin"/>）自己算。
        ///
        /// 一个触发元素可能同时撞上多个已挂元素：逐个结算，<b>触发元素被扣光就停</b>
        /// （它已经不存在了，剩下的组合自然不再共存）；还有剩就继续看下一对。
        /// </summary>
        private void ResolveReaction(int triggerElement)
        {
            var category = ElementReactionCategory.Instance;
            if (category == null || slots == null) return;

            for (int other = 1; other < slots.Length; other++)
            {
                if (other == triggerElement) continue;
                if (slots[other].Durability <= 0) continue;

                var reaction = category.GetReaction(triggerElement, other);
                if (reaction == null) continue;

                // 等量抵消：取两边当前量的较小值，双方各扣这么多（不是各扣全量，也不是按比例）
                int amount = slots[triggerElement].Durability;
                if (slots[other].Durability < amount) amount = slots[other].Durability;

                int takenTrigger = (int)Consume(triggerElement, amount);
                int takenOther = (int)Consume(other, amount);

                LastReactionId = reaction.Id;
                LastReactionTrigger = triggerElement;
                LastReactionOther = other;
                LastReactionTriggerConsumed = takenTrigger;
                LastReactionOtherConsumed = takenOther;

                Messager.Instance?.Broadcast(Id, MessageId.ElementReaction, new ElementReactionEvent
                {
                    Id = Id,
                    ReactionId = reaction.Id,
                    TriggerElement = triggerElement,
                    OtherElement = other,
                    TriggerConsumed = takenTrigger,
                    OtherConsumed = takenOther,
                });

                if (!Has(triggerElement)) return; // 触发元素已被反应抵消干净，没有下一对可言
            }
        }

        #endregion

        #region 消耗 / 查询

        /// <summary>
        /// 扣除元素量，<b>同步生效</b>：同一条 Actions 里后续 Action 立刻读到扣后值。
        /// <paramref name="amount"/> &lt;= 0 表示全扣（"反应吃掉全部元素"）；&gt; 0 时<b>向下取整</b>后再扣，
        /// 所以扣 2.9 = 扣 2 点，扣 0.9 = 一点不扣。
        /// </summary>
        /// <returns>实际扣掉的量；元素本来就没挂时返回 0。</returns>
        public float Consume(int elementId, float amount)
        {
            if (slots == null) return 0;
            if (elementId <= 0 || elementId >= slots.Length) return 0;

            int cur = slots[elementId].Durability;
            if (cur <= 0) return 0;

            int take = amount <= 0 ? cur : Mathf.FloorToInt(amount);
            if (take <= 0) return 0;   // 不足 1 点：扣不动，也不改动衰减余量
            if (take > cur) take = cur;

            cur -= take;
            slots[elementId].Durability = cur;
            if (cur <= 0)
            {
                slots[elementId].DecayAcc = 0;
                slots[elementId].ApplierId = 0;
                activeCount--;
                maskDirty = true;
                FlushMask();
            }
            return take;
        }

        /// <summary>
        /// 按配置元素 Id 扣量：元素不在表里返回 0，否则等同 <see cref="Consume"/>。
        /// <paramref name="amount"/> 会先向下取整（元素量不记小数），不足 1 点就一点不扣。
        /// </summary>
        /// <returns>实际扣掉的元素量。</returns>
        public float ConsumeByConfig(int elementId, float amount)
        {
            if (GetConfig(elementId) == null) return 0;
            return Consume(elementId, amount);
        }

        /// <summary>清空某个元素（净化 / 驱散）。</summary>
        public float Clear(int elementId)
        {
            return Consume(elementId, 0);
        }

        /// <summary>清空全部元素。</summary>
        public void ClearAll()
        {
            if (activeCount == 0 || slots == null) return;
            maskDirty = false;
            for (int i = 1; i < slots.Length; i++)
            {
                if (slots[i].Durability <= 0) continue;
                slots[i].Durability = 0;
                slots[i].DecayAcc = 0;
                slots[i].ApplierId = 0;
            }
            activeCount = 0;
            mask = 0;
            BroadcastChanged();
        }

        /// <summary>
        /// 当前元素量，<b>整数</b>（内部就是 int，这里返回 float 只为让占用比例等运算不必转型）。
        /// 没挂 / 非法 Id 返回 0。
        /// </summary>
        public float GetDurability(int elementId)
        {
            if (slots == null || elementId <= 0 || elementId >= slots.Length) return 0;
            return slots[elementId].Durability;
        }

        /// <summary>
        /// 元素量的上限（<see cref="ElementConfig.MaxDurability"/> 本来就是 int，元素量不记小数）。
        /// 没配返回 0。配合 <see cref="GetDurability"/> 算占用比例。
        /// </summary>
        public float GetMaxDurability(int elementId)
        {
            return GetConfig(elementId)?.MaxDurability ?? 0;
        }

        /// <summary>该元素是否挂着。</summary>
        public bool Has(int elementId)
        {
            return GetDurability(elementId) > 0;
        }

        /// <summary>最近一次附着该元素的来源实体 Id；没挂返回 0。</summary>
        public long GetApplierId(int elementId)
        {
            if (slots == null || elementId <= 0 || elementId >= slots.Length) return 0;
            return slots[elementId].Durability > 0 ? slots[elementId].ApplierId : 0;
        }

        /// <summary>已挂元素的位图（bit i = 元素 i）。只在"挂了哪些元素"变化时重算，逐帧调用不分配。</summary>
        public long GetAttachedMask()
        {
            if (maskDirty) FlushMask();
            return mask;
        }

        /// <summary>已挂元素个数。</summary>
        public int GetAttachedCount()
        {
            return activeCount;
        }

        #endregion

        #region 内部

        /// <summary>元素表对应的配置行；Id &lt;= 0 / 表未加载 / 表里没有都返回 null。</summary>
        private static ElementConfig GetConfig(int elementId)
        {
            if (elementId <= 0) return null;
            return ElementConfigCategory.Instance?.GetElement(elementId);
        }

        /// <summary>
        /// 按元素表长度建槽位数组。<b>返回 false 时调用方必须放弃本次操作</b>，
        /// 不要当成"元素量 0"继续往下算。
        /// </summary>
        private bool EnsureSlots()
        {
            if (slots != null) return true;

            var category = ElementConfigCategory.Instance;
            if (category == null)
            {
                WarnNoCategory();
                return false;
            }

            int max = category.GetMaxAttachId();
            if (max <= 0)
            {
                WarnNoTable();
                return false;
            }

            slots = new Slot[max + 1];
            return true;
        }

        private void FlushMask()
        {
            if (!maskDirty) return;
            maskDirty = false;

            long value = 0;
            if (slots != null)
            {
                int limit = slots.Length < 64 ? slots.Length : 64;
                for (int i = 1; i < limit; i++)
                {
                    if (slots[i].Durability > 0) value |= 1L << i;
                }
            }

            if (value == mask) return;
            mask = value;
            BroadcastChanged();
        }

        /// <summary>
        /// 只在"挂了哪些元素"变化时广播；元素量涨落<b>不广播</b> ——
        /// 否则每次命中都会发一条消息，订阅方（元素特效 / 血条图标）会被刷屏。
        /// 需要元素量精度的订阅方自己轮询 <see cref="GetDurability"/>。
        /// </summary>
        private void BroadcastChanged()
        {
            Messager.Instance?.Broadcast<long>(0, MessageId.ElementChanged, Id);
        }

        private static void WarnNoTable()
        {
            if (warnedNoTable) return;
            warnedNoTable = true;
            Log.Warning("[Element] 元素表里没有可附着的元素（耐久上限>0 的行），元素附着被忽略。" +
                        "请检查 Excel/ElementConfig@cs.xlsx 并重新导出 ElementConfigCategory.bytes");
        }

        private static bool warnedNoCategory;

        private static void WarnNoCategory()
        {
            if (warnedNoCategory) return;
            warnedNoCategory = true;
            Log.Error("[Element] 读不到 ElementConfigCategory：Assets/AssetsPackage/Config/ElementConfigCategory.bytes " +
                      "不存在或还没加载。元素附着全部失效（其余战斗逻辑不受影响）");
        }

        private static void WarnOnce(int elementId, string reason)
        {
            if (!warnedIds.Add(elementId)) return;
            Log.Warning($"[Element] {reason}");
        }

        #endregion
    }
}
