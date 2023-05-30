namespace TaoTie
{
    public class DoActionByExecuteMixin: AbilityMixin
    {
        public ConfigDoActionByExecuteMixin Config => baseConfig as ConfigDoActionByExecuteMixin;

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
            if (Config.Actions != null)
            {
                for (int i = 0; i < Config.Actions.Length; i++)
                {
                    Config.Actions[i].DoExecute(actorAbility.Parent.GetParent<Entity>(), actorAbility, actorModifier, null);
                }
            }
        }

        public override void Dispose()
        {
            if (actorModifier == null)
            {
                actorAbility.onExecute -= Execute;
            }
            else
            {
                actorModifier.onExecute -= Execute;
            }
            base.Dispose();
        }
    }
}