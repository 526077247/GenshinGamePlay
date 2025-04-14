namespace TaoTie
{
    public class DoActionByExecuteMixin: AbilityMixin<ConfigDoActionByExecuteMixin>
    {


        protected override void InitInternal(ActorAbility actorAbility, ActorModifier actorModifier, ConfigDoActionByExecuteMixin config)
        {
            if (actorModifier == null)
            {
                actorAbility.onExecute += Execute;
            }
            else
            {
                actorModifier.onExecute += Execute;
            }
            
        }
        
        private void Execute()
        {
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
            if (actorModifier == null)
            {
                actorAbility.onExecute -= Execute;
            }
            else
            {
                actorModifier.onExecute -= Execute;
            }
        }
    }
}