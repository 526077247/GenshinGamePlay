using System;
using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    public class AbilitySystem: IManager
    {
        public static AbilitySystem Instance;
        private Dictionary<Type, Type> configMixinType;

        public void Init()
        {
            Instance = this;
            configMixinType = AttributeManager.Instance.GetCreateTypeMap(TypeInfo<AbilityMixin>.Type);
        }

        public void Destroy()
        {
            configMixinType = null;
            Instance = null;
        }
        /// <summary>
        /// 创建Mixin
        /// </summary>
        /// <param name="actorAbility"></param>
        /// <param name="actorModifier"></param>
        /// <param name="config"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public AbilityMixin CreateAbilityMixin<T>(ActorAbility actorAbility, ActorModifier actorModifier,T config) where T:ConfigAbilityMixin
        {
            if(configMixinType.TryGetValue(config.GetType(), out var type))
            {
                var res = ObjectPool.Instance.Fetch(type) as AbilityMixin;
                res.Init(actorAbility,actorModifier,config);
                return res;
            }
            Log.Error($"CreateAbilityMixin失败， 未实现{config.GetType()}对应的Mixin");
            return null;
        }
    }
}