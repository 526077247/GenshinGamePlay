using UnityEngine;

namespace TaoTie
{
    public class DoActionOnInputMixin : AbilityMixin<ConfigDoActionOnInputMixin>,IUpdate
    {
        private Entity owner;

        private long timerId;
        protected override void InitInternal(ActorAbility actorAbility, ActorModifier actorModifier, ConfigDoActionOnInputMixin config)
        {
            owner = actorAbility.Parent.GetParent<Entity>();
            timerId = TimerManager.Instance.NewFrameTimer(TimerType.ComponentUpdate, this);
        }

        public void Update()
        {
            if (InputManager.Instance.GetKey(Config.KeyCode, Config.IgnoreUI))
            {
                ExecuteAction(owner);
            }
        }
        
        private void ExecuteAction(Entity target)
        {
            var actions = Config.Actions;
            if (actions != null)
            {
                var executer = GetActionExecuter();
                for (int i = 0; i < actions.Length; i++)
                {
                    actions[i].DoExecute(executer, actorAbility, actorModifier, target);
                }
            }
        }


        protected override void DisposeInternal()
        {
            TimerManager.Instance.Remove(ref timerId);
            owner = null;
        }
    }
}