namespace TaoTie
{
    public class ExecuteMixin: AbilityMixin
    {
        public ConfigExecuteMixin config => baseConfig as ConfigExecuteMixin;

        public override void Init(ActorAbility actorAbility, ActorModifier actorModifier, ConfigAbilityMixin config)
        {
            base.Init(actorAbility, actorModifier, config);
            if (actorModifier == null)
            {
                actorAbility.onExecute += Execute;
            }
            else
            {
                actorModifier.onExecute += Execute;
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
            actorAbility.onExecute -= Execute;
            actorModifier.onExecute -= Execute;
            base.Dispose();
        }
    }
}