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
            configPluginType = AttributeManager.Instance.GetCreateTypeMap(TypeInfo<BillboardPlugin>.Type);
        }
        /// <summary>
        /// preload一些常用hud到pool
        /// </summary>
        /// <returns></returns>
        public async ETTask PreloadLoadAsset()
        {
            await MaterialManager.Instance.PreLoadMaterial("Unit/Common/Materials/ProgressBar.mat");
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
        
        public BillboardPlugin CreateBillboardPlugin<T>(T config, BillboardComponent comp)where T :ConfigBillboardPlugin
        {
            if(configPluginType.TryGetValue(config.GetType(), out var runnerType))
            {
                var res = ObjectPool.Instance.Fetch(runnerType) as BillboardPlugin;
                res.Init(config, comp);
                return res;
            }
            Log.Error($"CreateBillboardPlugin失败， 未实现{config.GetType()}对应的Plugin");
            return null;
        }
    }
}