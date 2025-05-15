using UnityEngine;

namespace TaoTie
{
    public class DoActionOnTriggerMixin : AbilityMixin<ConfigDoActionOnTriggerMixin>
    {
        private Entity owner;
        private TriggerComponent triggerComponent;
        protected override void InitInternal(ActorAbility actorAbility, ActorModifier actorModifier, ConfigDoActionOnTriggerMixin config)
        {
            owner = actorAbility.Parent.GetParent<Entity>();
            triggerComponent = owner.GetComponent<TriggerComponent>();
            if (triggerComponent != null)
            {
                triggerComponent.OnTriggerExitEvt += ExecuteTriggerExit;
                triggerComponent.OnTriggerEnterEvt += ExecuteTriggerEnter;
                triggerComponent.OnTriggerEvt += ExecuteTrigger;
            }
        }
        
        private void ExecuteTriggerEnter(long targetId)
        {
            var actions = Config.TriggerEnterActions;
            if (actions != null)
            {
                var executer = GetActionExecuter();
                for (int i = 0; i < actions.Length; i++)
                {
                    var target = owner.Parent.Get(targetId);
                    if(target == null || target.IsDispose) continue;
                    actions[i].DoExecute(executer, actorAbility, actorModifier, target);
                }
            }
        }
        private void ExecuteTriggerExit(long targetId)
        {
            var actions = Config.TriggerExitActions;
            if (actions != null)
            {
                var executer = GetActionExecuter();
                for (int i = 0; i < actions.Length; i++)
                {
                    var target = owner.Parent.Get(targetId);
                    if(target == null || target.IsDispose) continue;
                    actions[i].DoExecute(executer, actorAbility, actorModifier, target);
                }
            }
        }
        private void ExecuteTrigger(long targetId)
        {
            var actions = Config.TriggerStayActions;
            if (actions != null)
            {
                var executer = GetActionExecuter();
                for (int i = 0; i < actions.Length; i++)
                {
                    var target = owner.Parent.Get(targetId);
                    if(target == null || target.IsDispose) continue;
                    actions[i].DoExecute(executer, actorAbility, actorModifier, target);
                }
            }
        }


        protected override void DisposeInternal()
        {
            if (triggerComponent != null)
            {
                triggerComponent.OnTriggerEvt -= ExecuteTrigger;
                triggerComponent.OnTriggerExitEvt -= ExecuteTriggerExit;
                triggerComponent.OnTriggerEnterEvt -= ExecuteTriggerEnter;
                triggerComponent = null;
            }
            owner = null;
        }
    }
}