using System.Collections.Generic;

namespace TaoTie
{
    public class BillboardComponent : Component, IComponent<ConfigBillboard>, IUpdate
    {
        private List<BillboardPlugin> plugins;
        public ConfigBillboard Config { get; private set; }
        public bool Enable { get; private set; }
        #region IComponent

        public void Init(ConfigBillboard config)
        {
            this.Config = config;
            Enable = true;
            plugins = new List<BillboardPlugin>();
            if (config != null && config.Plugins != null)
            {
                for (int i = 0; i < config.Plugins.Length; i++)
                {
                    var plugin = BillboardSystem.Instance.CreateBillboardPlugin(config.Plugins[i], this);
                    plugins.Add(plugin);
                }
            }
        }

        public void Destroy()
        {
            for (int i = 0; i < plugins.Count; i++)
            {
                plugins[i].Dispose();
            }

            plugins = null;
        }

        public void Update()
        {
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