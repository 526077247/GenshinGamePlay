namespace TaoTie
{
    public class DoActionAfterAddMixin : AbilityMixin
    {
        public ConfigDoActionAfterAddMixin Config => baseConfig as ConfigDoActionAfterAddMixin;

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
                actorAbility.afterAdd -= Execute;
            }
            else
            {
                actorModifier.afterAdd -= Execute;
            }
            base.Dispose();
        }
    }
}