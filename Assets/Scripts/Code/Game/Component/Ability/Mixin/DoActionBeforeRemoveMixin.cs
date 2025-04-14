namespace TaoTie
{
    public class DoActionBeforeRemoveMixin : AbilityMixin<ConfigDoActionBeforeRemoveMixin>
    {


        protected override void InitInternal(ActorAbility actorAbility, ActorModifier actorModifier, ConfigDoActionBeforeRemoveMixin config)
        {
            if (actorModifier == null)
            {
                actorAbility.beforeRemove += Execute;
            }
            else
            {
                actorModifier.beforeRemove += Execute;
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
                actorAbility.beforeRemove -= Execute;
            }
            else
            {
                actorModifier.beforeRemove -= Execute;
            }
        }
    }
}