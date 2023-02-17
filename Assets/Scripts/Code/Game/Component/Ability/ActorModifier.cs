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
        private long timerId;
        private long tillTime;

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
            res.Ability = ability;
            res.isDispose = false;
            res.Config = config;
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

        public override void AfterAdd()
        {
            base.AfterAdd();
            if (Config.Duration >= 0)
            {
                tillTime = GameTimerManager.Instance.GetTimeNow() + Config.Duration;
                timerId = GameTimerManager.Instance.NewOnceTimer(tillTime, TimerType.ModifierExpired, this);
            }

            if (Config.Properties != null)
            {
                var numC = Parent.GetParent<Entity>().GetComponent<NumericComponent>();
                if (numC != null)
                {
                    for (int i = 0; i < Config.Properties.Length; i++)
                    {
                        var old = numC.GetAsFloat(Config.Properties[i].NumericType);
                        numC.Set(Config.Properties[i].NumericType,old + Config.Properties[i].Value);
                    }
                }
            }
        }

        public override void BeforeRemove()
        {
            GameTimerManager.Instance.Remove(ref timerId);
            if (Config.Properties != null)
            {
                var numC = Parent.GetParent<Entity>().GetComponent<NumericComponent>();
                if (numC != null)
                {
                    for (int i = 0; i < Config.Properties.Length; i++)
                    {
                        var old = numC.GetAsFloat(Config.Properties[i].NumericType);
                        numC.Set(Config.Properties[i].NumericType,old - Config.Properties[i].Value);
                    }
                }
            }
            base.BeforeRemove();
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