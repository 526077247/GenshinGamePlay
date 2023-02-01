using System;

namespace TaoTie
{
    public abstract class AbilityMixin:IDisposable
    {

        protected ConfigAbilityMixin baseConfig;
        protected ActorAbility actorAbility;

        public virtual void Init(ActorAbility actorAbility,ConfigAbilityMixin config)
        {
            this.baseConfig = config;
            this.actorAbility = actorAbility;
        }
        
        public virtual void Dispose()
        {
            actorAbility = null;
            baseConfig = null;
            ObjectPool.Instance.Recycle(this);
        }
    }
}