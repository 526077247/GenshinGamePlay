using System;
using System.Collections.Generic;

namespace TaoTie
{
    public sealed class ActorAbility : IDisposable
    {
        private bool isDispose = true;
        public event Action afterAdd;
        public event Action beforeRemove;

        public ConfigAbility Config { get; private set; }
        public AbilityComponent Parent { get; private set; }

        private ListComponent<AbilityMixin> mixins;
        private DictionaryComponent<string, ConfigAbilityModifier> modifierConfigs;

        public static ActorAbility Create(ConfigAbility config, AbilityComponent component)
        {
            var res = ObjectPool.Instance.Fetch(typeof(ActorAbility)) as ActorAbility;
            res.Config = config;
            res.Parent = component;
            res.mixins = ListComponent<AbilityMixin>.Create();
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
            if (isDispose) return;
            isDispose = true;
            Parent.RemoveAbility(this);

            for (int i = 0; i < mixins.Count; i++)
            {
                mixins[i].Dispose();
            }

            mixins.Dispose();
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