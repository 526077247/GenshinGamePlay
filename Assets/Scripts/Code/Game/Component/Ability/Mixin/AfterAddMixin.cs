namespace TaoTie
{
    public class AfterAddMixin : AbilityMixin
    {
        public ConfigAfterAddMixin config => baseConfig as ConfigAfterAddMixin;

        public override void Init(Ability ability, ConfigAbilityMixin config)
        {
            base.Init(ability, config);
            ability.afterAdd += Excute;
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
            ability.afterAdd -= Excute;
            base.Dispose();
        }
    }
}