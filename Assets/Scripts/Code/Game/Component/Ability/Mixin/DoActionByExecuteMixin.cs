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
            if (Config.Actions != null)
            {
                var executer = GetActionExecuter();
                for (int i = 0; i < Config.Actions.Length; i++)
                {
                    Config.Actions[i].DoExecute(executer, actorAbility, actorModifier, executer);
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