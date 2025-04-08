namespace TaoTie
{
    public class DoActionAfterLoadObjectMixin: AbilityMixin<ConfigDoActionAfterLoadObjectMixin>
    {

        private UnitModelComponent unitModelComponent;
        private Entity owner;

        protected override void InitInternal(ActorAbility actorAbility, ActorModifier actorModifier, ConfigDoActionAfterLoadObjectMixin config)
        {
            owner = actorAbility.Parent.GetParent<Entity>();
            unitModelComponent = owner?.GetComponent<UnitModelComponent>();
            if (unitModelComponent != null) CheckObjLoad().Coroutine();
        }

        private async ETTask CheckObjLoad()
        {
            await unitModelComponent.WaitLoadGameObjectOver();
            if (owner == null || owner.IsDispose) return;
            if (Config.Actions!=null)
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
            unitModelComponent = null;
            owner = null;
        }
    }
}