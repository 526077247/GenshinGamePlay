using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    public class BillboardComponent : Component, IComponent<ConfigBillboard>, IUpdate
    {
        private List<BillboardPlugin> plugins;
        public ConfigBillboard Config { get; private set; }
        public bool Enable { get; private set; }

        public float Scale{ get; private set; }
        
        public Transform Target{ get; private set; }
        
        #region IComponent

        public void Init(ConfigBillboard config)
        {
            Config = config;
            Enable = true;
            plugins = new List<BillboardPlugin>();
            Scale = 1;
            if (Config != null && Config.Plugins != null)
            {
                InitInternal().Coroutine();
            }
        }

        private async ETTask InitInternal()
        {
            var goh = GetComponent<GameObjectHolderComponent>();
            await goh.WaitLoadGameObjectOver();
            if(goh.IsDispose || IsDispose) return;
            Target = goh.GetCollectorObj<Transform>(Config.AttachPoint);
            for (int i = 0; i < Config.Plugins.Length; i++)
            {
                var plugin = BillboardSystem.Instance.CreateBillboardPlugin(Config.Plugins[i], this);
                plugins.Add(plugin);
            }
        }

        public void Destroy()
        {
            for (int i = 0; i < plugins.Count; i++)
            {
                plugins[i].Dispose();
            }

            Target = null;
            plugins = null;
        }

        public void Update()
        {
            if (Target != null)
            {
                var camera = CameraManager.Instance.MainCamera();
                if (camera != null)
                {
                    var distance2Camera = Vector3.Distance(camera.transform.position, Target.position);
                    Scale = Mathf.Lerp(0.1f, 6f, (distance2Camera-0.5f)/40);
                }
            }
            for (int i = 0; i < plugins.Count; i++)
            {
                plugins[i].Update();
            }
        }

        #endregion

        public void SetEnable(bool enable)
        {
            Enable = enable;
        }
    }
}