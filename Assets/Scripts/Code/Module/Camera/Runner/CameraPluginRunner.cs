using System;

namespace TaoTie
{
    public abstract class CameraPluginRunner: IDisposable
    {
        public abstract void Init(ConfigCameraPlugin config,NormalCameraState state);
        public abstract void Update();
        public abstract void Dispose();
    }
    public abstract class CameraPluginRunner<T>: CameraPluginRunner where T: ConfigCameraPlugin
    {
        protected T config { get; private set; }
        protected NormalCameraState state { get; private set; }

        protected CameraStateData data => state.Data;
        public sealed override void Init(ConfigCameraPlugin config,NormalCameraState state)
        {
            this.config = config as T;
            this.state = state;
            InitInternal();
        }
        protected abstract void InitInternal();
        public sealed override void Update()
        {
            UpdateInternal();
        }

        protected abstract void UpdateInternal();
        
        public sealed override void Dispose()
        {
            DisposeInternal();
            config = null;
            state = null;
            ObjectPool.Instance.Recycle(this);
        }

        protected abstract void DisposeInternal();
    }
}