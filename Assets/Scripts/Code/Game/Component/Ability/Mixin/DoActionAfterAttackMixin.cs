namespace TaoTie
{
    public class DoActionAfterAttackMixin: AbilityMixin
    {
        
        public ConfigDoActionAfterAttackMixin ConfigDoAction => baseConfig as ConfigDoActionAfterAttackMixin;

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
            if (ConfigDoAction.Actions != null)
            {
                for (int i = 0; i < ConfigDoAction.Actions.Length; i++)
                {
                    ConfigDoAction.Actions[i].DoExecute(actorAbility.Parent.GetParent<Entity>(), actorAbility, actorModifier, other.GetParent<Entity>());
                }
            }
        }
    }
}