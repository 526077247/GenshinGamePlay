namespace TaoTie
{
    public class DoActionAfterLoadObjectMixin: AbilityMixin<ConfigDoActionAfterLoadObjectMixin>
    {

        private ModelComponent modelComponent;
        private Entity owner;

        protected override void InitInternal(ActorAbility actorAbility, ActorModifier actorModifier, ConfigDoActionAfterLoadObjectMixin config)
        {
            owner = actorAbility.Parent.GetParent<Entity>();
            modelComponent = owner?.GetComponent<ModelComponent>();
            if (modelComponent != null) CheckObjLoad().Coroutine();
        }

        private async ETTask CheckObjLoad()
        {
            await modelComponent.WaitLoadGameObjectOver();
            if (owner == null || owner.IsDispose) return;
            if (Config.Actions!=null)
            {
                for (int i = 0; i < Config.Actions.Length; i++)
                {
                    Config.Actions[i].DoExecute(owner, actorAbility, actorModifier, null);
                }
            }
        }
        protected override void DisposeInternal()
        {
            modelComponent = null;
            owner = null;
        }
    }
}