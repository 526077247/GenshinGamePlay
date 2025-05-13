using System;

namespace TaoTie
{
    [Creatable]
    public abstract class AbilityMixin:IDisposable
    {
        protected ConfigAbilityMixin baseConfig;
        protected ActorAbility actorAbility;
        protected ActorModifier actorModifier;
        public abstract void Init(ActorAbility actorAbility, ActorModifier actorModifier, ConfigAbilityMixin config);
        public abstract void Dispose();

        /// <summary>
        /// 如果是Modifier则为Modifier的持有者，否则为Ability持有者
        /// </summary>
        /// <returns></returns>
        protected Entity GetActionExecuter()
        {
            if (actorModifier == null) return actorAbility.Parent.GetParent<Entity>();
            return actorModifier.Parent.GetParent<Entity>();
        }
    }
    

    public abstract class AbilityMixin<T> : AbilityMixin where T : ConfigAbilityMixin
    {
        public T Config => baseConfig as T;

        public sealed override void Init(ActorAbility actorAbility, ActorModifier actorModifier, ConfigAbilityMixin config)
        {
            this.baseConfig = config;
            this.actorAbility = actorAbility;
            this.actorModifier = actorModifier;
            InitInternal(actorAbility, actorModifier, config as T);
        }

        public sealed override void Dispose()
        {
            DisposeInternal();
            actorAbility = null;
            baseConfig = null;
            actorModifier = null;
            ObjectPool.Instance.Recycle(this);
        }

        protected abstract void InitInternal(ActorAbility actorAbility, ActorModifier actorModifier, T config);
        
        protected abstract void DisposeInternal();
    }
}