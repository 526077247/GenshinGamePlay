namespace TaoTie
{
    public class DoActionAfterAttackMixin: AbilityMixin<ConfigDoActionAfterAttackMixin>
    {
        

        private CombatComponent combat;

        protected override void InitInternal(ActorAbility actorAbility, ActorModifier actorModifier, ConfigDoActionAfterAttackMixin config)
        {
            combat = actorAbility.Parent.GetParent<Entity>().GetComponent<CombatComponent>();
            if (combat != null)
            {
                combat.afterAttack += Execute;
            }

        }

        protected override void DisposeInternal()
        {
            if (combat != null)
            {
                combat.afterAttack -= Execute;
            }

            combat = null;
        }

        private void Execute(AttackResult attackResult,CombatComponent other)
        {
            if (Config.Actions != null)
            {
                for (int i = 0; i < Config.Actions.Length; i++)
                {
                    Config.Actions[i].DoExecute(actorAbility.Parent.GetParent<Entity>(), actorAbility, actorModifier, other.GetParent<Entity>());
                }
            }
        }
    }
}