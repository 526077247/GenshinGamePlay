using System;

namespace TaoTie
{
    public abstract class AbilityMixin:IDisposable
    {

        protected ConfigAbilityMixin baseConfig;
        protected ActorAbility actorAbility;
        protected ActorModifier actorModifier;
        public virtual void Init(ActorAbility actorAbility, ActorModifier actorModifier, ConfigAbilityMixin config)
        {
            this.baseConfig = config;
            this.actorAbility = actorAbility;
            this.actorModifier = actorModifier;
        }
        
        public virtual void Dispose()
        {
            actorAbility = null;
            baseConfig = null;
            actorModifier = null;
            ObjectPool.Instance.Recycle(this);
        }
    }
}