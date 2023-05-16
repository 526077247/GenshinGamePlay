using System;

namespace TaoTie
{
    public abstract class CameraPluginRunner: IDisposable
    {
        public abstract void Update();
        public abstract void Dispose();
    }
    public abstract class CameraPluginRunner<T>: CameraPluginRunner where T: ConfigCameraPlugin
    {
        protected T plugin;

        public sealed override void Update()
        {
            UpdateInternal();
        }

        protected abstract void UpdateInternal();
        
        public void Init(T config)
        {
            plugin = config;
        }
    }
}