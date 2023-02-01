namespace TaoTie
{
    public class BeforeRemoveMixin : AbilityMixin
    {
        public ConfigBeforeRemoveMixin config => baseConfig as ConfigBeforeRemoveMixin;

        public override void Init(ActorAbility actorAbility, ActorModifier actorModifier, ConfigAbilityMixin config)
        {
            base.Init(actorAbility, actorModifier, config);
            if (actorModifier == null)
            {
                actorAbility.beforeRemove += Excute;
            }
            else
            {
                actorModifier.beforeRemove += Excute;
            }
            
        }
        
        private void Excute()
        {
            if (config.Actions != null)
            {
                for (int i = 0; i < config.Actions.Length; i++)
                {
                    config.Actions[i].DoExecute(actorAbility.Parent.GetParent<Entity>(), actorAbility, null);
                }
            }
        }

        public override void Dispose()
        {
            actorAbility.beforeRemove -= Excute;
            actorModifier.beforeRemove -= Excute;
            base.Dispose();
        }
    }
}