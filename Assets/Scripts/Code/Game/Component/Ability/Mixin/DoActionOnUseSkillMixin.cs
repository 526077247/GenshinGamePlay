using UnityEngine;

namespace TaoTie
{
    public class DoActionOnUseSkillMixin : AbilityMixin<ConfigDoActionOnUseSkillMixin>
    {
        private Entity owner;
        private SkillComponent skillComponent;
        protected override void InitInternal(ActorAbility actorAbility, ActorModifier actorModifier, ConfigDoActionOnUseSkillMixin config)
        {
            owner = actorAbility.Parent.GetParent<Entity>();
            skillComponent = owner.GetComponent<SkillComponent>();
            if (skillComponent != null)
            {
                skillComponent.OnDoSkillEvt += OnSkill;
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

        private void OnSkill(int skillID)
        {
            if (skillID == Config.SkillId)
            {
                ExecuteAction(owner);
            }
        } 

        protected override void DisposeInternal()
        {
            if (skillComponent != null)
            {
                skillComponent.OnDoSkillEvt -= OnSkill;
                skillComponent = null;
            }
            owner = null;
        }
    }
}