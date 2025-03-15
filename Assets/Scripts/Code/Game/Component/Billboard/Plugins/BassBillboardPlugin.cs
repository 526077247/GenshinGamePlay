using System;
using UnityEngine;

namespace TaoTie
{
    public abstract class BassBillboardPlugin: IDisposable
    {
        protected BillboardComponent billboardComponent;
        protected Transform target => billboardComponent.Target;

        public virtual void Init(BillboardComponent comp)
        {
            this.billboardComponent = comp;
        }
        
        public abstract void Update();
        public abstract void Dispose();
    }
}