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
        /// </summary>
        public int StackCount { get; private set; }

        /// <summary>
        /// Properties 已经按几层应用过，用于摘除时一次扣干净。
        /// </summary>
        private int appliedStack;

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
            res.Init(applierID,component);
            res.Id = IdGenerater.Instance.GenerateId();
            res.Ability = ability;
            res.isDispose = false;
            res.Config = config;
            res.timerId = 0;
            res.tillTime = 0;
            res.StackCount = 1;
            res.appliedStack = 0;
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
            StackCount++;
            ApplyProperties(1);
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