namespace TaoTie
{
    public class DoActionOnInteeTouchMixin : AbilityMixin<ConfigDoActionOnInteeTouchMixin>
    {
        private InteeComponent intee;
        private Entity owner;

        protected override void InitInternal(ActorAbility actorAbility, ActorModifier actorModifier, ConfigDoActionOnInteeTouchMixin config)
        {
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


        protected override void DisposeInternal()
        {
            if (intee != null)
            {
                intee.OnTouch -= Execute;
                intee = null;
            }
            
            owner = null;
        }
    }
}