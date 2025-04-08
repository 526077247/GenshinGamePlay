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
                actorAbility.beforeRemove -= Execute;
            }
            else
            {
                actorModifier.beforeRemove -= Execute;
            }
        }
    }
}