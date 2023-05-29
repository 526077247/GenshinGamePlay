namespace TaoTie
{
    public class DoActionBeforeRemoveMixin : AbilityMixin
    {
        public ConfigDoActionBeforeRemoveMixin Config => baseConfig as ConfigDoActionBeforeRemoveMixin;

        public override void Init(ActorAbility actorAbility, ActorModifier actorModifier, ConfigAbilityMixin config)
        {
            base.Init(actorAbility, actorModifier, config);
            if (actorModifier == null)
            {
                actorAbility.beforeRemove += Execute;
            }
            else
            {
                actorModifier.beforeRemove += Execute;
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
            actorAbility.beforeRemove -= Execute;
            actorModifier.beforeRemove -= Execute;
            base.Dispose();
        }
    }
}