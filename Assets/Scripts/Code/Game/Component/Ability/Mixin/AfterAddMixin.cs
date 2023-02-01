namespace TaoTie
{
    public class AfterAddMixin : AbilityMixin
    {
        public ConfigAfterAddMixin config => baseConfig as ConfigAfterAddMixin;

        public override void Init(ActorAbility actorAbility, ConfigAbilityMixin config)
        {
            base.Init(actorAbility, config);
            actorAbility.afterAdd += Excute;
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
            actorAbility.afterAdd -= Excute;
            base.Dispose();
        }
    }
}