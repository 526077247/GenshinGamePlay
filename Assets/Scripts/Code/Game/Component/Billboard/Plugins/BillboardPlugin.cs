using System;
using UnityEngine;

namespace TaoTie
{
    public abstract class BillboardPlugin: IDisposable
    {
        public abstract void Init(ConfigBillboardPlugin config, BillboardComponent comp);
        public abstract void Update();
        public abstract void Dispose();
    }
    
    public abstract class BillboardPlugin<T>: BillboardPlugin where T : ConfigBillboardPlugin
    {
        protected T config { get; private set; }

        protected BillboardComponent billboardComponent;

        protected Transform target => billboardComponent.Target;
        
        public sealed override void Init(ConfigBillboardPlugin config, BillboardComponent comp)
        {
            this.config = config as T;
            this.billboardComponent = comp;
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
            ObjectPool.Instance.Recycle(this);
        }

        protected abstract void DisposeInternal();
    }
}