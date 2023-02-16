namespace TaoTie
{
    public class DoActionAfterBeAttackMixin: AbilityMixin
    {
        public ConfigDoActionAfterBeAttackMixin config => baseConfig as ConfigDoActionAfterBeAttackMixin;

        private CombatComponent _combatComponent;
        public override void Init(ActorAbility actorAbility, ActorModifier actorModifier, ConfigAbilityMixin config)
        {
            base.Init(actorAbility, actorModifier, config);
            _combatComponent = actorAbility.Parent.GetParent<Entity>().GetComponent<CombatComponent>();
            if (_combatComponent != null)
            {
                _combatComponent.afterBeAttack += Execute;
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
                _combatComponent.afterBeAttack -= Execute;
                _combatComponent = null;
            }
            base.Dispose();
        }
    }
}