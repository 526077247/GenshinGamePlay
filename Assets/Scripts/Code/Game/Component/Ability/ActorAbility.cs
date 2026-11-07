using System;
using System.Collections.Generic;

namespace TaoTie
{
    public sealed class ActorAbility :BaseActorActionContext,INumericReplace
    {
        public ConfigAbility Config { get; private set; }
        
        private DictionaryComponent<string, ConfigAbilityModifier> modifierConfigs;
        private DynDictionary dynDictionary;
        public static ActorAbility Create(long applierID,ConfigAbility config, AbilityComponent component)
        {
            var res = ObjectPool.Instance.Fetch<ActorAbility>();
            res.Init(applierID,component);
            res.dynDictionary = DynDictionary.Create();
            res.Config = config;
            res.modifierConfigs = DictionaryComponent<string, ConfigAbilityModifier>.Create();
            res.isDispose = false;

            if (config.AbilitySpecials != null)             
            {
                foreach (var item in config.AbilitySpecials)
                {
                    res.dynDictionary.Set(item.Key,item.Value);
                }
            }
            
            if (config.AbilityMixins != null)
            {
                // ability 级 Mixin 以前没有校验入口；监听类的漏填（WatchTags=None）只在运行时静默失效，
                // 必须在装载期就吵出来。modifier 级由 ValidateModifier 内部统一处理。
                GameplayTagMeta.ValidateMixins(config.AbilityMixins, config.AbilityName, GameplayTags.None);

                for (int i = 0; i < config.AbilityMixins.Length; i++)
                {
                    var mixin = AbilitySystem.Instance.CreateAbilityMixin(res, null, config.AbilityMixins[i]);
                    res.mixins.Add(mixin);
                }
            }

            if (config.Modifiers != null)
            {
                for (int i = 0; i < config.Modifiers.Length; i++)
                {
                    var modifier = config.Modifiers[i];
                    // 装载期校验标记配置：写错就在加载时报错，别等行为诡异。
                    // 每次装载都查（每条 modifier 两次位运算），只在配置有误时打日志
                    GameplayTagMeta.ValidateModifier(modifier, config.AbilityName);
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
            modifierConfigs = null;
            dynDictionary.Dispose();
            dynDictionary = null;
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


        /// <summary>
        /// 获取变量
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public float GetReplaceValue(string key)
        {
            return dynDictionary.Get(key);
        }
        
        
        /// <summary>
        /// 获取变量
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public void SetSpecials(string key,float value)
        {
            dynDictionary.Set(key,value);
        }
    }
}