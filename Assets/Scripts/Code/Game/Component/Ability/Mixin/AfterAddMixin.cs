namespace TaoTie
{
    public class AfterAddMixin : AbilityMixin
    {
        public ConfigAfterAddMixin config => baseConfig as ConfigAfterAddMixin;

        public override void Init(ActorAbility actorAbility, ActorModifier actorModifier, ConfigAbilityMixin config)
        {
            base.Init(actorAbility, actorModifier, config);
            if (actorModifier == null)
            {
                actorAbility.afterAdd += Execute;
            }
            else
            {
                actorModifier.afterAdd += Execute;
            }
            
        }

        private void Execute()
        {
            if (config.Actions != null)
            {
                for (int i = 0; i < config.Actions.Length; i++)
                {
                    config.Actions[i].DoExecute(actorAbility.Parent.GetParent<Entity>(), actorAbility, actorModifier, null);
                }
            }
        }

        public override void Dispose()
        {
            actorAbility.afterAdd -= Execute;
            actorModifier.afterAdd -= Execute;
            base.Dispose();
        }
    }
}