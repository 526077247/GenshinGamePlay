namespace TaoTie
{
    public class DoActionAfterLoadObjectMixin: AbilityMixin
    {
        public ConfigDoActionAfterLoadObjectMixin Config => baseConfig as ConfigDoActionAfterLoadObjectMixin;

        private GameObjectHolderComponent holderComponent;
        private Entity owner;
        public override void Init(ActorAbility actorAbility, ActorModifier actorModifier, ConfigAbilityMixin config)
        {
            base.Init(actorAbility, actorModifier, config);
            owner = actorAbility.Parent.GetParent<Entity>();
            holderComponent = owner?.GetComponent<GameObjectHolderComponent>();
            if (holderComponent != null) CheckObjLoad().Coroutine();
        }

        private async ETTask CheckObjLoad()
        {
            await holderComponent.WaitLoadGameObjectOver();
            if (owner == null || owner.IsDispose) return;
            if (Config.Actions!=null)
            {
                for (int i = 0; i < Config.Actions.Length; i++)
                {
                    Config.Actions[i].DoExecute(owner, actorAbility, actorModifier, null);
                }
            }
        }
        public override void Dispose()
        {
            holderComponent = null;
            owner = null;
            base.Dispose();
        }
    }
}