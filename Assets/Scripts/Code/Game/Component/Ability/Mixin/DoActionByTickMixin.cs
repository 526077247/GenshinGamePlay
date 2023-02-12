using System;

namespace TaoTie
{
    public class DoActionByTickMixin : AbilityMixin
    {
        
        [Timer(TimerType.TickMixin)]
        public class TickMixinTimer:ATimer<DoActionByTickMixin>
        {
            public override void Run(DoActionByTickMixin t)
            {
                try
                {
                    t.Execute();
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
            }
        }
        
        public ConfigDoActionByTickMixin ConfigDoActionBy => baseConfig as ConfigDoActionByTickMixin;

        private long timerId;
        public override void Init(ActorAbility actorAbility, ActorModifier actorModifier, ConfigAbilityMixin config)
        {
            base.Init(actorAbility, actorModifier, config);
            timerId = GameTimerManager.Instance.NewRepeatedTimer(this.ConfigDoActionBy.Interval, TimerType.TickMixin, this);
            if (this.ConfigDoActionBy.TickFirstOnAdd)
            {
                Execute();
            }
        }
        
        private void Execute()
        {
            if (ConfigDoActionBy.Actions != null)
            {
                for (int i = 0; i < ConfigDoActionBy.Actions.Length; i++)
                {
                    ConfigDoActionBy.Actions[i].DoExecute(actorAbility.Parent.GetParent<Entity>(), actorAbility, actorModifier, null);
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