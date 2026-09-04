using UnityEngine;

namespace TaoTie
{
    public class DoActionOnFsmTimelineTriggerMixin : AbilityMixin<ConfigDoActionOnFsmTimelineTriggerMixin>
    {
        private Entity owner;
        private FsmComponent fsm;
        protected override void InitInternal(ActorAbility actorAbility, ActorModifier actorModifier, ConfigDoActionOnFsmTimelineTriggerMixin config)
        {
            owner = actorAbility.Parent.GetParent<Entity>();
            fsm = owner.GetComponent<FsmComponent>();
            if (fsm != null)
                fsm.OnFsmTimelineTriggerEvt += OnFsmTimelineTriggerEvt;
        }
        
        private void OnFsmTimelineTriggerEvt(string triggerId)
        {
            if(triggerId != Config.TriggerId) return;
            var actions = Config.Actions;
            if (actions != null)
            {
                var executer = GetActionExecuter();
                for (int i = 0; i < actions.Length; i++)
                {
                    var target = owner;
                    if(target == null || target.IsDispose) continue;
                    actions[i].DoExecute(executer, actorAbility, actorModifier, target);
                }
            }
        }
        protected override void DisposeInternal()
        {
            if (fsm != null)
            {
                fsm.OnFsmTimelineTriggerEvt -= OnFsmTimelineTriggerEvt;
            }
            owner = null;
        }
    }
}