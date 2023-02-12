namespace TaoTie
{
    public class AfterAttackMixin: AbilityMixin
    {
        
        public ConfigAfterAttackMixin config => baseConfig as ConfigAfterAttackMixin;

        private CombatComponent combat;
        public override void Init(ActorAbility actorAbility, ActorModifier actorModifier, ConfigAbilityMixin config)
        {
            base.Init(actorAbility, actorModifier, config);
            combat = actorAbility.Parent.GetParent<Entity>().GetComponent<CombatComponent>();
            if (combat != null)
            {
                combat.afterAttack += Execute;
            }

        }

        public override void Dispose()
        {
            if (combat != null)
            {
                combat.afterAttack -= Execute;
            }

            combat = null;
            base.Dispose();
        }

        private void Execute(AttackResult attackResult,CombatComponent other)
        {
            if (config.Actions != null)
            {
                for (int i = 0; i < config.Actions.Length; i++)
                {
                    config.Actions[i].DoExecute(actorAbility.Parent.GetParent<Entity>(), actorAbility, actorModifier, other.GetParent<Entity>());
                }
            }
        }
    }
}