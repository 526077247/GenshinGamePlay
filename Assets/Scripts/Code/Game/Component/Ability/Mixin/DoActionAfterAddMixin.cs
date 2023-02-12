namespace TaoTie
{
    public class DoActionAfterAddMixin : AbilityMixin
    {
        public ConfigDoActionAfterAddMixin ConfigDoAction => baseConfig as ConfigDoActionAfterAddMixin;

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
            if (ConfigDoAction.Actions != null)
            {
                for (int i = 0; i < ConfigDoAction.Actions.Length; i++)
                {
                    ConfigDoAction.Actions[i].DoExecute(actorAbility.Parent.GetParent<Entity>(), actorAbility, actorModifier, null);
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