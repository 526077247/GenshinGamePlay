using System;
using UnityEngine;

namespace TaoTie
{
    [Creatable]
    public abstract class BillboardPlugin : BassBillboardPlugin
    {
        public sealed override void Init(BillboardComponent comp)
        {
            base.Init(comp);
        }

        public abstract void Init(ConfigBillboardPlugin config, BillboardComponent comp);
    }
    public abstract class BillboardPlugin<T>: BillboardPlugin where T : ConfigBillboardPlugin
    {
        protected T config { get; private set; }
        
        public override void Init(ConfigBillboardPlugin config, BillboardComponent comp)
        {
            this.config = config as T;
            base.Init(comp);
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