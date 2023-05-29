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
        
        public ConfigDoActionByTickMixin Config => baseConfig as ConfigDoActionByTickMixin;

        private long timerId;
        public override void Init(ActorAbility actorAbility, ActorModifier actorModifier, ConfigAbilityMixin config)
        {
            base.Init(actorAbility, actorModifier, config);
            timerId = GameTimerManager.Instance.NewRepeatedTimer(this.Config.Interval, TimerType.TickMixin, this);
            if (this.Config.TickFirstOnAdd)
            {
                Execute();
            }
        }
        
        private void Execute()
        {
            if (Config.Actions != null)
            {
                for (int i = 0; i < Config.Actions.Length; i++)
                {
                    Config.Actions[i].DoExecute(actorAbility.Parent.GetParent<Entity>(), actorAbility, actorModifier, null);
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