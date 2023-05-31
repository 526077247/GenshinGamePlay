using System.Collections.Generic;

namespace TaoTie
{
    public class BillboardComponent : Component, IComponent<ConfigBillboard>, IUpdate
    {
        private List<BillboardPlugin> plugins;
        private ConfigBillboard config;

        #region IComponent

        public void Init(ConfigBillboard config)
        {
            this.config = config;
            plugins = new List<BillboardPlugin>();
            if (config != null && config.Plugins != null)
            {
                for (int i = 0; i < config.Plugins.Length; i++)
                {
                    plugins.Add(BillboardSystem.Instance.CreateBillboardPlugin(config.Plugins[i]));
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