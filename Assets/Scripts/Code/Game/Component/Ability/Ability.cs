using System;
using System.Collections.Generic;

namespace TaoTie
{
    public sealed class Ability: IDisposable
    {
        
        public event Action afterAdd;
        public event Action beforeRemove;
        
        private ConfigAbility config;
        private AbilityComponent parent;
        
        private ListComponent<AbilityMixin> mixins;
        
        public static Ability Create(ConfigAbility config, AbilityComponent component)
        {
            var res = ObjectPool.Instance.Fetch(typeof(Ability)) as Ability;
            res.config = config;
            res.parent = component;
            res.mixins = ListComponent<AbilityMixin>.Create();
            if (config.AbilityMixins != null)
            {
                for (int i = 0; i < config.AbilityMixins.Length; i++)
                {
                    var mixin = config.AbilityMixins[i].CreateAbilityMixin(res);
                    res.mixins.Add(mixin);
                }
            }

            return res;
        }
        
        public void AfterAdd()
        {
            afterAdd?.Invoke();
        }
        public void BeforeRemove()
        {
            beforeRemove?.Invoke();
        }

        public void Dispose()
        {
            for (int i = 0; i < mixins.Count; i++)
            {
                mixins[i].Dispose();
            }
            mixins.Dispose();
            config = null;
            ObjectPool.Instance.Recycle(this);
        }
    }
}