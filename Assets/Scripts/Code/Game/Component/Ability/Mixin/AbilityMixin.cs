using System;

namespace TaoTie
{
    public abstract class AbilityMixin:IDisposable
    {

        protected ConfigAbilityMixin baseConfig;
        protected Ability ability;

        public virtual void Init(Ability ability,ConfigAbilityMixin config)
        {
            this.baseConfig = config;
            this.ability = ability;
        }
        
        public virtual void Dispose()
        {
            ability = null;
            baseConfig = null;
            ObjectPool.Instance.Recycle(this);
        }
    }
}