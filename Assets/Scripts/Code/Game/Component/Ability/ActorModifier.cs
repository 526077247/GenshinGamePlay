using System;
using System.Collections.Generic;

namespace TaoTie
{
    public sealed class ActorModifier : BaseActorActionContext
    {
        [Timer(TimerType.ModifierExpired)]
        public class ModifierExpiredTimer : ATimer<ActorModifier>
        {
            public override void Run(ActorModifier t)
            {
                try
                {
                    t.Dispose();
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
            }
        }
        
        public ConfigAbilityModifier Config{ get; private set; }
        public ActorAbility Ability { get; private set; }
        /// <summary>
        /// 一次攻击/效果实例的唯一标识,复用对象池每次创建重新生成
        /// </summary>
        public long Id { get; private set; }
        private long timerId;
        private long tillTime;

        /// <summary>
        /// 当前层数，1 起步。
        ///
        /// Multiple 的每个实例恒为 1（层数由实例个数体现）；
        /// Stack 类型共用一个实例，靠 <see cref="TryAddStack"/> 自增。
        /// 上限不在这里，而在 GameplayTagMeta（上限属于标记，不属于 modifier）。
        ///
        /// <b>这是层数的唯一真相源</b>：Properties 的按层缩放、
        /// <c>ControlComponent.tagStacks</c> 的聚合、<c>TagStackValue</c> 的读取全由它驱动。
        /// 状态字典里另配一个"层数" key 不会与它联动，别把一件事记成两份。
        /// </summary>
        public int StackCount { get; private set; }

        /// <summary>
        /// Properties 已经按几层应用过，用于摘除时一次扣干净。
        /// </summary>
        private int appliedStack;

        /// <summary>
        /// 到期时刻（毫秒）；无限时长（Duration &lt; 0）时为 0，排序时视为最晚。
        /// 用于逐层衰减：减 N 层优先消耗最早到期的那几层，与 Multiple 每层各自倒计时的直觉一致。
        /// </summary>
        public long TillTime => tillTime;

        /// <summary>
        /// 层数已减到 0、等待帧末摘除。
        ///
        /// <b>为什么摘除要延迟、数字却不能延迟</b>：减层 Action 常挂在 modifier 自己的
        /// Tick / 状态变化 / 受击 Mixin 里，外层可能正在遍历 <c>AbilityComponent.Modifiers</c>
        /// 链表，同步 Dispose 会改动集合；但「改数字」不动集合，必须立即生效 —— 否则同一条
        /// Actions 里后续的 <c>TagStackValue</c>、伤害结算读到的还是减之前的层数。
        ///
        /// 待摘除这一帧里 <c>ControlComponent.Rebuild</c> 会跳过 0 层实例，
        /// 它既不贡献标记，也不贡献剥夺，不会出现"标记在但层数为 0"。
        /// </summary>
        public bool IsPendingRemove { get; private set; }

        /// <summary>
        /// 创建
        /// </summary>
        /// <param name="applierID">施加者id</param>
        /// <param name="config"></param>
        /// <param name="ability"></param>
        /// <param name="component"></param>
        /// <returns></returns>
        public static ActorModifier Create(long applierID, ConfigAbilityModifier config, ActorAbility ability,
            AbilityComponent component)
        {
            var res = ObjectPool.Instance.Fetch<ActorModifier>();
            res.Init(applierID, component);
            res.Id = IdGenerater.Instance.GenerateId();
            res.Ability = ability;
            res.isDispose = false;
            res.Config = config;
            res.timerId = 0;
            res.tillTime = 0;
            res.StackCount = 1;
            res.appliedStack = 0;
            res.IsPendingRemove = false;

            // 状态初值必须在创建 Mixin 之前落库：Mixin 的 InitInternal 会读它做校验
            // （比如吸收类 Mixin 检查 key 是否配了），读到 0 会误判成「没配盾」。
            if (config.ModifierSpecials != null && config.ModifierSpecials.Count > 0)
            {
                foreach (var item in config.ModifierSpecials)
                {
                    if (string.IsNullOrEmpty(item.Key)) continue;
                    res.SetSpecial(item.Key, item.Value);
                }
            }

            if (config.Mixins != null)
            {
                for (int i = 0; i < config.Mixins.Length; i++)
                {
                    var mixin = AbilitySystem.Instance.CreateAbilityMixin(ability, res, config.Mixins[i]);
                    res.mixins.Add(mixin);
                }
            }

            return res;
        }

        public override void AfterAdd()
        {
            base.AfterAdd();
            if (Config.Duration >= 0)
            {
                tillTime = GameTimerManager.Instance.GetTimeNow() + Config.Duration;
                timerId = GameTimerManager.Instance.NewOnceTimer(tillTime, TimerType.ModifierExpired, this);
            }

            ApplyProperties(StackCount);
            AttachElementOnAdd();
        }

        /// <summary>
        /// <b>非伤害效果的元素附着</b>：Modifier 挂到实体上的这一刻，按配置给宿主挂元素。
        ///
        /// 为什么挂在这里而不是伤害结算里：给队友挂 buff、给自己上状态、场地效果这类<b>没有伤害结算</b>的
        /// 效果同样要能附着元素。它们的共同点是「都有一个 Modifier 挂到目标身上」，所以挂在
        /// <see cref="AfterAdd"/> 就能覆盖全部非伤害场景，不需要为每种效果单开一条路径。
        ///
        /// 伤害型的元素附着不走这里：那由 <see cref="ConfigAttackProperty.Element"/> /
        /// <c>ElementDurability</c> 决定，在 <see cref="AttackHelper.DamageClose"/> 末尾附着。
        /// <b>两者都配会让同一个元素在 Modifier 挂上和命中时各挂一次</b>，配置时二选一。
        ///
        /// 只在挂上时触发一次，叠层（<see cref="TryAddStack"/>）不会重复挂 ——
        /// "每一层各挂一次"没有对应的直觉，需要就配多条 Modifier。
        /// </summary>
        private void AttachElementOnAdd()
        {
            if (Config == null || Config.Element <= 0 || Config.ElementDurability <= 0) return;

            var entity = Parent?.GetParent<Entity>();
            if (entity == null || entity.IsDispose) return;

            // 宿主没有元素体系（Gadget / 场景物等）时是常态，静默跳过
            entity.GetComponent<ElementComponent>()?.Attach(Config.Element, Config.ElementDurability, ApplierID);
        }

        public override void BeforeRemove()
        {
            GameTimerManager.Instance.Remove(ref timerId);
            //广播清理受击方记录的该攻击实例的命中记录,复用对象池时可避免旧记录误命中
            Messager.Instance?.Broadcast<long>(0, MessageId.ClearHitRecord, Id);
            ApplyProperties(-appliedStack);
            base.BeforeRemove();
        }


        /// <summary>
        /// 按层数增量应用 / 回退 Properties。
        ///
        /// delta &gt; 0 加层，delta &lt; 0 回退（摘除时传 -appliedStack，一次扣干净）。
        /// 不做全量重算：GetAsFloat→Set 往返会带来精度漂移，
        /// 而增量只加差值。
        /// </summary>
        private void ApplyProperties(int delta)
        {
            if (delta == 0 || appliedStack + delta < 0 || Config?.Properties == null) return;

            var entity = Parent.GetParent<Entity>();
            var numC = entity?.GetComponent<NumericComponent>();
            if (numC == null) return;

            for (int i = 0; i < Config.Properties.Length; i++)
            {
                var prop = Config.Properties[i];
                var old = numC.GetAsFloat(prop.NumericType);
                numC.Set(prop.NumericType, old + prop.Value * delta);
            }
            appliedStack += delta;
        }

        /// <summary>
        /// 叠一层。
        /// 上限校验在 <see cref="AbilityComponent"/>（它按标记池的余量判），
        /// 这里只负责上层已经同意的那一次自增。
        /// </summary>
        public void TryAddStack()
        {
            // 同帧「减到 0 → 又加一层」：取消待摘除，否则帧末会把刚续上的层一起摘掉。
            IsPendingRemove = false;
            StackCount++;
            ApplyProperties(1);
        }

        /// <summary>
        /// <b>同步</b>削减层数，返回实际减掉的层数；<b>不摘除实例</b>。
        ///
        /// 归零时只置 <see cref="IsPendingRemove"/>，由 <see cref="AbilityComponent"/> 帧末统一
        /// <see cref="FlushPendingRemove"/>。这样"数字立即生效、结构延后变更"，
        /// 同一条 Actions 里的后续 Action 能读到减后层数，也不会在别人遍历链表时改集合。
        ///
        /// 属性按层增量回退，不做全量重算（避免 GetAsFloat→Set 的精度漂移）。
        /// 归零后 <c>appliedStack</c> 已回到 0，随后 <see cref="BeforeRemove"/> 里的
        /// <c>ApplyProperties(-appliedStack)</c> 是 -0 空操作，不会双扣。
        /// </summary>
        public int ReduceStack(int take)
        {
            if (isDispose || Parent == null || take <= 0 || StackCount <= 0) return 0;

            int actual = Math.Min(take, StackCount);
            StackCount -= actual;
            ApplyProperties(-actual);
            if (StackCount == 0) IsPendingRemove = true;
            return actual;
        }

        /// <summary>
        /// 帧末由 <see cref="AbilityComponent"/> 调用，真正摘除 0 层实例。
        /// 若这一帧内又被 <see cref="TryAddStack"/> 续上，则只清标记、不摘除。
        /// </summary>
        public void FlushPendingRemove()
        {
            if (!IsPendingRemove || isDispose) return;
            IsPendingRemove = false;
            if (StackCount > 0) return;
            Dispose();
        }

        public void SetDuration(long duration)
        {
            GameTimerManager.Instance.Remove(ref timerId);
            tillTime = GameTimerManager.Instance.GetTimeNow() + duration;
            timerId = GameTimerManager.Instance.NewOnceTimer(tillTime, TimerType.ModifierExpired, this);
        }
        
        public void AddDuration(long duration)
        {
            GameTimerManager.Instance.Remove(ref timerId);
            tillTime += duration;
            timerId = GameTimerManager.Instance.NewOnceTimer(tillTime, TimerType.ModifierExpired, this);
        }
        
        public override void Dispose()
        {
            if (isDispose) return;
            isDispose = true;
            Parent.RemoveModifier(this);

            base.Dispose();
            
            Config = null;
            ObjectPool.Instance.Recycle(this);
        }
    }
}
