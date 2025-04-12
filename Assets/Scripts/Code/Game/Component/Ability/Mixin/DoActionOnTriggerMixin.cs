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
            }
        }
        
        private void ExecuteTriggerEnter(Entity target)
        {
            if (Config.TriggerEnterActions != null)
            {
                var executer = GetActionExecuter();
                for (int i = 0; i < Config.TriggerEnterActions.Length; i++)
                {
                    Config.TriggerEnterActions[i].DoExecute(executer, actorAbility, actorModifier, target);
                }
            }
        }
        private void ExecuteTriggerExit(Entity target)
        {
            if (Config.TriggerExitActions != null)
            {
                var executer = GetActionExecuter();
                for (int i = 0; i < Config.TriggerExitActions.Length; i++)
                {
                    Config.TriggerExitActions[i].DoExecute(executer, actorAbility, actorModifier, target);
                }
            }
        }
        

        protected override void DisposeInternal()
        {
            if (triggerComponent != null)
            {
                triggerComponent.OnTriggerExitEvt -= ExecuteTriggerExit;
                triggerComponent.OnTriggerEnterEvt -= ExecuteTriggerEnter;
                triggerComponent = null;
            }
            owner = null;
        }
    }
}