using System.Collections.Generic;

namespace TaoTie
{
    public class BillboardComponent : Component, IComponent<ConfigBillboard>, IUpdate
    {
        private List<BillboardPlugin> plugins;
        public ConfigBillboard Config { get; private set; }

        #region IComponent

        public void Init(ConfigBillboard config)
        {
            this.Config = config;
            plugins = new List<BillboardPlugin>();
            if (config != null && config.Plugins != null)
            {
                for (int i = 0; i < config.Plugins.Length; i++)
                {
                    plugins.Add(BillboardSystem.Instance.CreateBillboardPlugin(config.Plugins[i], this));
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
    }
}