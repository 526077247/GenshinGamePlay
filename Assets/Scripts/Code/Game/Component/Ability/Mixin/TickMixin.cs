using System;

namespace TaoTie
{
    public class TickMixin : AbilityMixin
    {
        
        [Timer(TimerType.TickMixin)]
        public class TickMixinTimer:ATimer<TickMixin>
        {
            public override void Run(TickMixin t)
            {
                try
                {
                    t.Excute();
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
            }
        }
        
        public ConfigTickMixin config => baseConfig as ConfigTickMixin;

        private long timerId;
        public override void Init(ActorAbility actorAbility, ActorModifier actorModifier, ConfigAbilityMixin config)
        {
            base.Init(actorAbility, actorModifier, config);
            timerId = GameTimerManager.Instance.NewRepeatedTimer(this.config.Interval, TimerType.TickMixin, this);
            if (this.config.TickFirstOnAdd)
            {
                Excute();
            }
        }
        
        private void Excute()
        {
            if (config.Actions != null)
            {
                for (int i = 0; i < config.Actions.Length; i++)
                {
                    config.Actions[i].DoExecute(actorAbility.Parent.GetParent<Entity>(), actorAbility, actorModifier, null);
                }
            }
        }

        public override void Dispose()
        {
            GameTimerManager.Instance.Remove(ref timerId);
            base.Dispose();
        }

    }
}