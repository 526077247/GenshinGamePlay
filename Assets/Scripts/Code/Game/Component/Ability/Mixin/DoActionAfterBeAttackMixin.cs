namespace TaoTie
{
    public class DoActionAfterBeAttackMixin: AbilityMixin<ConfigDoActionAfterBeAttackMixin>
    {

        private CombatComponent combatComponent;

        protected override void InitInternal(ActorAbility actorAbility, ActorModifier actorModifier, ConfigDoActionAfterBeAttackMixin config)
        {
            combatComponent = actorAbility.Parent.GetParent<Entity>().GetComponent<CombatComponent>();
            if (combatComponent != null)
            {
                combatComponent.afterBeAttack += Execute;
            }

        }

        private void Execute(AttackResult result, CombatComponent other)
        {
            if (Config.Actions != null)
            {
                for (int i = 0; i < Config.Actions.Length; i++)
                {
                    Config.Actions[i].DoExecute(actorAbility.Parent.GetParent<Entity>(), actorAbility, actorModifier, other.GetParent<Entity>());
                }
            }
        }

        protected override void DisposeInternal()
        {
            if (combatComponent != null)
            {
                combatComponent.afterBeAttack -= Execute;
                combatComponent = null;
            }
        }
    }
}