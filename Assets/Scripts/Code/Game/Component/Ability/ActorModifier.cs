using System;

namespace TaoTie
{
    public sealed class ActorModifier: IDisposable
    {
        private bool isDispose = true;
        [Timer(TimerType.ModifierExpired)]
        public class TickMixinTimer:ATimer<ActorModifier>
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
        
        private ConfigAbilityModifier config;
        private AbilityComponent parent;
        private ActorAbility ability;
        private long applierID;
        private long timerId;
        
        private ListComponent<AbilityMixin> mixins;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="applierID">施加者id</param>
        /// <param name="config"></param>
        /// <param name="ability"></param>
        /// <param name="component"></param>
        /// <returns></returns>
        public static ActorModifier Create(long applierID, ConfigAbilityModifier config, ActorAbility ability, AbilityComponent component)
        {
            var res = ObjectPool.Instance.Fetch(typeof(ActorModifier)) as ActorModifier;
            res.config = config;
            res.applierID = applierID;
            res.ability = ability;
            res.parent = component;
            res.mixins = ListComponent<AbilityMixin>.Create();
            res.isDispose = false;
            if (config.Mixins != null)
            {
                for (int i = 0; i < config.Mixins.Length; i++)
                {
                    var mixin = config.Mixins[i].CreateAbilityMixin(ability);
                    res.mixins.Add(mixin);
                }
            }

            return res;
        }
        
        public void AfterAdd()
        {
            afterAdd?.Invoke();
            if (config.Duration >= 0)
            {
                timerId = GameTimerManager.Instance.NewOnceTimer(config.Duration, TimerType.ModifierExpired, this);
            }
        }
        public void BeforeRemove()
        {
            GameTimerManager.Instance.Remove(ref timerId);
            beforeRemove?.Invoke();
        }

        public void Dispose()
        {
            if(isDispose) return;
            isDispose = true;
            parent.RemoveModifier(this);
            
            for (int i = 0; i < mixins.Count; i++)
            {
                mixins[i].Dispose();
            }
            mixins.Dispose();
            config = null;
            ObjectPool.Instance.Recycle(this);
        }

    }
}