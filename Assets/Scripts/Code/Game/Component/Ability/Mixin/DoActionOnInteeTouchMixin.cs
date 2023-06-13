namespace TaoTie
{
    public class DoActionOnInteeTouchMixin : AbilityMixin
    {
        public ConfigDoActionOnInteeTouchMixin Config => baseConfig as ConfigDoActionOnInteeTouchMixin;
        
        private InteeComponent intee;
        private Entity owner;
        public override void Init(ActorAbility actorAbility, ActorModifier actorModifier, ConfigAbilityMixin config)
        {
            base.Init(actorAbility, actorModifier, config);
            owner = actorAbility.Parent.GetParent<Entity>();
            intee = owner?.GetComponent<InteeComponent>();
            if (intee != null)
            {
                intee.OnTouch += Execute;
            }
        }

        

        private void Execute(int configId)
        {
            if(configId != Config.LocalId) return;
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
            if (intee != null)
            {
                intee.OnTouch -= Execute;
                intee = null;
            }
            
            owner = null;
            base.Dispose();
        }
    }
}