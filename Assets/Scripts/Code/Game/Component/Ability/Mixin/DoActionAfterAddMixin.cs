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