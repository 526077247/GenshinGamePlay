using System;
using System.Collections.Generic;

namespace TaoTie
{
    public sealed class ActorAbility :BaseActorActionContext
    {
        public ConfigAbility Config { get; private set; }
        
        private DictionaryComponent<string, ConfigAbilityModifier> modifierConfigs;

        public static ActorAbility Create(long applierID,ConfigAbility config, AbilityComponent component)
        {
            var res = ObjectPool.Instance.Fetch(TypeInfo<ActorAbility>.Type) as ActorAbility;
            res.Init(applierID,component);
            res.Config = config;
            res.modifierConfigs = DictionaryComponent<string, ConfigAbilityModifier>.Create();
            res.isDispose = false;
            if (config.AbilityMixins != null)
            {
                for (int i = 0; i < config.AbilityMixins.Length; i++)
                {
                    var mixin = config.AbilityMixins[i].CreateAbilityMixin(res, null);
                    res.mixins.Add(mixin);
                }
            }

            if (config.Modifiers != null)
            {
                for (int i = 0; i < config.Modifiers.Length; i++)
                {
                    var modifier = config.Modifiers[i];
                    res.modifierConfigs.Add(modifier.ModifierName, modifier);
                }
            }

            return res;
        }

        public override void Dispose()
        {
            if (isDispose) return;
            isDispose = true;
            Parent.RemoveAbility(this);
            
            base.Dispose();
            
            modifierConfigs.Dispose();
            Config = null;
            ObjectPool.Instance.Recycle(this);
        }


        /// <summary>
        /// 根据名字取modifier
        /// </summary>
        /// <param name="name"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        public bool TryGetConfigAbilityModifier(string name, out ConfigAbilityModifier config)
        {
            return modifierConfigs.TryGetValue(name, out config);
        }
    }
}