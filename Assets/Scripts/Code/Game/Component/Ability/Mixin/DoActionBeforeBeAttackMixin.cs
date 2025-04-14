namespace TaoTie
{
    public class DoActionBeforeBeAttackMixin: AbilityMixin<ConfigDoActionBeforeBeAttackMixin>
    {

        private CombatComponent combatComponent;

        protected override void InitInternal(ActorAbility actorAbility, ActorModifier actorModifier, ConfigDoActionBeforeBeAttackMixin config)
        {
            combatComponent = actorAbility.Parent.GetParent<Entity>().GetComponent<CombatComponent>();
            if (combatComponent != null)
            {
                combatComponent.beforeBeAttack += Execute;
            }

        }

        private void Execute(AttackResult result, CombatComponent other)
        {
            var actions = Config.Actions;
            if (actions != null)
            {
                var executer = GetActionExecuter();
                var target = other.GetParent<Entity>();
                for (int i = 0; i < actions.Length; i++)
                {
                    actions[i].DoExecute(executer, actorAbility, actorModifier, target);
                }
            }
        }

        protected override void DisposeInternal()
        {
            if (combatComponent != null)
            {
                combatComponent.beforeBeAttack -= Execute;
                combatComponent = null;
            }
        }
    }
}