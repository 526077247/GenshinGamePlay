using System;

namespace TaoTie
{
    public abstract class ArrangePlugin : IDisposable
    {
        protected ConfigArrange configArrange;
        protected UnitModelComponent UnitModel;
        public bool IsDispose { get;protected set; }
        public abstract void Update();

        public abstract void Init(ConfigArrange config, UnitModelComponent unitModelComponent);

        public abstract void Dispose();
    }

    public abstract class ArrangePlugin<T> : ArrangePlugin where T : ConfigArrange
    {
        protected T Config => configArrange as T;

        public sealed override void Init(ConfigArrange config, UnitModelComponent component)
        {
            UnitModel = component;
            configArrange = config;
            IsDispose = false;
            InitInternal();
        }

        public sealed override void Dispose()
        {
            DisposeInternal();
            UnitModel = null;
            configArrange = null;
            IsDispose = true;
            ObjectPool.Instance.Recycle(this);
        }

        protected abstract void InitInternal();
        
        protected abstract void DisposeInternal();
    }
}