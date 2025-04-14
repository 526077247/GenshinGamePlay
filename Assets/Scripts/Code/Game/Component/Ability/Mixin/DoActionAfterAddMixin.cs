namespace TaoTie
{
    public class DoActionAfterAddMixin : AbilityMixin<ConfigDoActionAfterAddMixin>
    {
        protected override void InitInternal(ActorAbility actorAbility, ActorModifier actorModifier, ConfigDoActionAfterAddMixin config)
        {
            if (actorModifier == null)
            {
                actorAbility.afterAdd += Execute;
            }
            else
            {
                actorModifier.afterAdd += Execute;
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
                actorAbility.afterAdd -= Execute;
            }
            else
            {
                actorModifier.afterAdd -= Execute;
            }
        }
    }
}