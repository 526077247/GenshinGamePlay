using System;
using System.Collections;
using System.Collections.Generic;

namespace TaoTie
{
    public class BillboardSystem : IManager, IUpdate
    {
        public static BillboardSystem Instance { get; private set; }
        
        private Dictionary<Type, Type> configPluginType;
        
        public void Init()
        {
            Instance = this;
            configPluginType = new Dictionary<Type, Type>();
            var allTypes = AssemblyManager.Instance.GetTypes();
            var pluginType = TypeInfo<BillboardPlugin>.Type;
            foreach (var item in allTypes)
            {
                var type = item.Value;
                if (!type.IsAbstract && pluginType.IsAssignableFrom(type))
                {
                    configPluginType.Add(type.BaseType.GenericTypeArguments[0],type);
                }
            }
        }
        /// <summary>
        /// preload一些常用hud到pool
        /// </summary>
        /// <returns></returns>
        public async ETTask PreloadLoadAsset()
        {
            await ETTask.CompletedTask;
        }

        public void Destroy()
        {
            Instance = null;
        }

        public void Update()
        {
            var hudView = UIManager.Instance.GetWindow<UIDamageView>(1);
            if (hudView != null)
            {
                hudView.Update();
            }
        }
        
        public BillboardPlugin CreateBillboardPlugin<T>(T config)where T :ConfigBillboardPlugin
        {
            if(configPluginType.TryGetValue(config.GetType(), out var runnerType))
            {
                var res = ObjectPool.Instance.Fetch(runnerType) as BillboardPlugin;
                res.Init(config);
                return res;
            }
            Log.Error($"CreateBillboardPlugin失败， 未实现{config.GetType()}对应的Plugin");
            return null;
        }
    }
}