namespace TaoTie
{
    public class BeforeRemoveMixin : AbilityMixin
    {
        public ConfigBeforeRemoveMixin config => baseConfig as ConfigBeforeRemoveMixin;

        public override void Init(Ability ability, ConfigAbilityMixin config)
        {
            base.Init(ability, config);
            ability.beforeRemove += Excute;
        }
        
        private void Excute()
        {
            if (config.Actions != null)
            {
                for (int i = 0; i < config.Actions.Length; i++)
                {
                    config.Actions[i].DoExecute();
                }
            }
        }

        public override void Dispose()
        {
            ability.beforeRemove -= Excute;
            base.Dispose();
        }
    }
}