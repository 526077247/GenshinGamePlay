using System;
using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    public class MoveSystem: IManager
    {
        public static MoveSystem Instance;
        private Dictionary<Type, Type> configType;

        public void Init()
        {
            Instance = this;
            configType = AttributeManager.Instance.GetCreateTypeMap(TypeInfo<MoveStrategy>.Type);
        }

        public void Destroy()
        {
            configType = null;
            Instance = null;
        }
        /// <summary>
        /// 创建MoveStrategy
        /// </summary>
        /// <param name="moveC"></param>
        /// <param name="config"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public MoveStrategy CreateMoveStrategy<T>(MoveComponent moveC, T config) where T: ConfigMoveStrategy
        {
            if(configType.TryGetValue(config.GetType(), out var type))
            {
                var res = ObjectPool.Instance.Fetch(type) as MoveStrategy;
                res.Init(moveC, config);
                return res;
            }
            Log.Error($"CreateMoveStrategy失败， 未实现{config.GetType()}对应的Strategy");
            return null;
        }
        
        /// <summary>
        /// 创建MoveStrategy
        /// </summary>
        /// <param name="moveC"></param>
        /// <param name="config"></param>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="P1">参数1</typeparam>
        /// <returns></returns>
        public MoveStrategy CreateMoveStrategy<T,P1>(MoveComponent moveC, T config, P1 p1) where T: ConfigMoveStrategy
        {
            if(configType.TryGetValue(config.GetType(), out var type))
            {
                var res = ObjectPool.Instance.Fetch(type) as MoveStrategy;
                res.Init(moveC, config, p1);
                return res;
            }
            Log.Error($"CreateMoveStrategy失败， 未实现{config.GetType()}对应的Strategy");
            return null;
        }
    }
}