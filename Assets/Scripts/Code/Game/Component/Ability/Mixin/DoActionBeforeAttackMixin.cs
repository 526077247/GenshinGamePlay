namespace TaoTie
{
    public class DoActionBeforeAttackMixin: AbilityMixin
    {
        public ConfigDoActionBeforeAttackMixin Config => baseConfig as ConfigDoActionBeforeAttackMixin;

        private CombatComponent combatComponent;
        public override void Init(ActorAbility actorAbility, ActorModifier actorModifier, ConfigAbilityMixin config)
        {
            base.Init(actorAbility, actorModifier, config);
            combatComponent = actorAbility.Parent.GetParent<Entity>().GetComponent<CombatComponent>();
            if (combatComponent != null)
            {
                combatComponent.beforeAttack += Execute;
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

        public override void Dispose()
        {
            if (combatComponent != null)
            {
                combatComponent.beforeAttack -= Execute;
                combatComponent = null;
            }
            base.Dispose();
        }
    }
}