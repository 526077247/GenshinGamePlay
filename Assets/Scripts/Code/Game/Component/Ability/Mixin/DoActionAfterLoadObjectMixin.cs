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
            var actions = Config.Actions;
            if (actions!=null)
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
            unitModelComponent = null;
            owner = null;
        }
    }
}