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
        
        public ConfigDoActionByTickMixin ConfigDoAction => baseConfig as ConfigDoActionByTickMixin;

        private long timerId;
        public override void Init(ActorAbility actorAbility, ActorModifier actorModifier, ConfigAbilityMixin config)
        {
            base.Init(actorAbility, actorModifier, config);
            timerId = GameTimerManager.Instance.NewRepeatedTimer(this.ConfigDoAction.Interval, TimerType.TickMixin, this);
            if (this.ConfigDoAction.TickFirstOnAdd)
            {
                Execute();
            }
        }
        
        private void Execute()
        {
            if (ConfigDoAction.Actions != null)
            {
                for (int i = 0; i < ConfigDoAction.Actions.Length; i++)
                {
                    ConfigDoAction.Actions[i].DoExecute(actorAbility.Parent.GetParent<Entity>(), actorAbility, actorModifier, null);
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