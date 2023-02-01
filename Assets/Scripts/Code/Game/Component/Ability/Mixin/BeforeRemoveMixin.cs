namespace TaoTie
{
    public class BeforeRemoveMixin : AbilityMixin
    {
        public ConfigBeforeRemoveMixin config => baseConfig as ConfigBeforeRemoveMixin;

        public override void Init(ActorAbility actorAbility, ConfigAbilityMixin config)
        {
            base.Init(actorAbility, config);
            actorAbility.beforeRemove += Excute;
        }
        
        private void Excute()
        {
            if (config.Actions != null)
            {
                for (int i = 0; i < config.Actions.Length; i++)
                {
                    config.Actions[i].DoExecute(actorAbility,null);
                }
            }
        }

        public override void Dispose()
        {
            actorAbility.beforeRemove -= Excute;
            base.Dispose();
        }
    }
}