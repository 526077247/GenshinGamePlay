using System;

namespace TaoTie
{
    public sealed class ActorModifier : IDisposable
    {
        private bool isDispose = true;

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

        public event Action afterAdd;
        public event Action beforeRemove;

        public ConfigAbilityModifier Config{ get; private set; }
        public AbilityComponent Parent{ get; private set; }
        public ActorAbility Ability { get; private set; }
        public long ApplierID { get; private set; }
        private long timerId;
        private long tillTime;
        private ListComponent<AbilityMixin> mixins;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="applierID">施加者id</param>
        /// <param name="config"></param>
        /// <param name="ability"></param>
        /// <param name="component"></param>
        /// <returns></returns>
        public static ActorModifier Create(long applierID, ConfigAbilityModifier config, ActorAbility ability,
            AbilityComponent component)
        {
            var res = ObjectPool.Instance.Fetch(typeof(ActorModifier)) as ActorModifier;
            res.Config = config;
            res.ApplierID = applierID;
            res.Ability = ability;
            res.Parent = component;
            res.mixins = ListComponent<AbilityMixin>.Create();
            res.isDispose = false;
            if (config.Mixins != null)
            {
                for (int i = 0; i < config.Mixins.Length; i++)
                {
                    var mixin = config.Mixins[i].CreateAbilityMixin(ability, res);
                    res.mixins.Add(mixin);
                }
            }

            return res;
        }

        public void AfterAdd()
        {
            afterAdd?.Invoke();
            if (Config.Duration >= 0)
            {
                tillTime = GameTimerManager.Instance.GetTimeNow() + Config.Duration;
                timerId = GameTimerManager.Instance.NewOnceTimer(tillTime, TimerType.ModifierExpired, this);
            }
        }

        public void BeforeRemove()
        {
            GameTimerManager.Instance.Remove(ref timerId);
            beforeRemove?.Invoke();
        }

        public void Dispose()
        {
            if (isDispose) return;
            isDispose = true;
            Parent.RemoveModifier(this);

            for (int i = 0; i < mixins.Count; i++)
            {
                mixins[i].Dispose();
            }

            mixins.Dispose();
            Config = null;
            ObjectPool.Instance.Recycle(this);
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
    }
}