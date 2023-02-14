namespace TaoTie
{
    public class DoActionByExecuteMixin: AbilityMixin
    {
        public ConfigDoActionByExecuteMixin ConfigDoActionBy => baseConfig as ConfigDoActionByExecuteMixin;

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
            if (ConfigDoActionBy.Actions != null)
            {
                for (int i = 0; i < ConfigDoActionBy.Actions.Length; i++)
                {
                    ConfigDoActionBy.Actions[i].DoExecute(actorAbility.Parent.GetParent<Entity>(), actorAbility, actorModifier, null);
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