using System;

namespace TaoTie
{
    public abstract class CameraPluginRunner: IDisposable
    {
        public abstract void Init(ConfigCameraPlugin config);
        public abstract void Update();
        public abstract void Dispose();
    }
    public abstract class CameraPluginRunner<T>: CameraPluginRunner where T: ConfigCameraPlugin
    {
        protected T plugin { get; private set; }

        public sealed override void Init(ConfigCameraPlugin config)
        {
            plugin = config as T;
            InitInternal(plugin);
        }
        protected abstract void InitInternal(T config);
        public sealed override void Update()
        {
            UpdateInternal();
        }

        protected abstract void UpdateInternal();
        
        public sealed override void Dispose()
        {
            plugin = null;
            DisposeInternal();
            ObjectPool.Instance.Recycle(this);
        }

        protected abstract void DisposeInternal();
    }
}