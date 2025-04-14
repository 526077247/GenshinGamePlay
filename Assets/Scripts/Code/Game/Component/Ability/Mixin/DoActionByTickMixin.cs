using System;

namespace TaoTie
{
    public class DoActionByTickMixin : AbilityMixin<ConfigDoActionByTickMixin>
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
        

        private long timerId;

        protected override void InitInternal(ActorAbility actorAbility, ActorModifier actorModifier, ConfigDoActionByTickMixin config)
        {
            if (Config.EveryFrame)
            {
                timerId = GameTimerManager.Instance.NewFrameTimer(TimerType.TickMixin, this);
            }
            else
            {
                timerId = GameTimerManager.Instance.NewRepeatedTimer(this.Config.Interval, TimerType.TickMixin, this);
            }
            if (this.Config.TickFirstOnAdd)
            {
                Execute();
            }
        }
        
        private void Execute()
        {
            var actions = Config.Actions;
            if (actions != null)
            {
                var executer = GetActionExecuter();
                for (int i = 0; i < actions.Length; i++)
                {
                    actions[i].DoExecute(executer, actorAbility, actorModifier, executer);
                }
            }
        }

        protected override void DisposeInternal()
        {
            GameTimerManager.Instance.Remove(ref timerId);
        }

    }
}