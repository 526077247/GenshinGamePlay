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
            var actions = Config.Actions;
            if (actions != null)
            {
                var executer = GetActionExecuter();
                for (int i = 0; i < actions.Length; i++)
                {
                    actions[i].DoExecute(executer, actorAbility, actorModifier, executer);
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