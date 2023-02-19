namespace TaoTie
{
    public class DoActionBeforeAttackMixin: AbilityMixin
    {
        public ConfigDoActionBeforeAttackMixin config => baseConfig as ConfigDoActionBeforeAttackMixin;

        private CombatComponent _combatComponent;
        public override void Init(ActorAbility actorAbility, ActorModifier actorModifier, ConfigAbilityMixin config)
        {
            base.Init(actorAbility, actorModifier, config);
            _combatComponent = actorAbility.Parent.GetParent<Entity>().GetComponent<CombatComponent>();
            if (_combatComponent != null)
            {
                _combatComponent.beforeAttack += Execute;
            }

        }

        private void Execute(AttackResult result, CombatComponent other)
        {
            if (config.Actions != null)
            {
                for (int i = 0; i < config.Actions.Length; i++)
                {
                    config.Actions[i].DoExecute(actorAbility.Parent.GetParent<Entity>(), actorAbility, actorModifier, other.GetParent<Entity>());
                }
            }
        }

        public override void Dispose()
        {
            if (_combatComponent != null)
            {
                _combatComponent.beforeAttack -= Execute;
                _combatComponent = null;
            }
            base.Dispose();
        }
    }
}