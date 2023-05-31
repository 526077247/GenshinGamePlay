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
        protected GameObject obj { get; private set; }

        protected BillboardComponent billboardComponent;
        
        public sealed override void Init(ConfigBillboardPlugin config, BillboardComponent comp)
        {
            this.config = config as T;
            this.billboardComponent = comp;
            LoadObj().Coroutine();
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
            if (obj != null)
            {
                GameObjectPoolManager.Instance.RecycleGameObject(obj);
                obj = null;
            }
            config = null;
            ObjectPool.Instance.Recycle(this);
        }

        protected abstract void DisposeInternal();
        
        private async ETTask LoadObj()
        {
            if(string.IsNullOrWhiteSpace(config.PrefabPath)) return;
            var goh = billboardComponent.GetParent<Entity>().GetComponent<GameObjectHolderComponent>();
            var obj = await GameObjectPoolManager.Instance.GetGameObjectAsync(config.PrefabPath);
            if (billboardComponent.IsDispose || goh.IsDispose)
            {
                GameObjectPoolManager.Instance.RecycleGameObject(obj);
                return;
            }
            await goh.WaitLoadGameObjectOver();
            if (billboardComponent.IsDispose || goh.IsDispose)
            {
                GameObjectPoolManager.Instance.RecycleGameObject(obj);
                return;
            }
            var pointer = goh.GetCollectorObj<Transform>(billboardComponent.Config.AttachPoint);
            if (pointer == null)
            {
                GameObjectPoolManager.Instance.RecycleGameObject(obj);
                return;
            }

            this.obj = obj;
            this.obj.transform.SetParent(pointer);
            this.obj.transform.localPosition = billboardComponent.Config.Offset + config.Offset;
            this.obj.transform.localScale = Vector3.one;
            OnGameObjectLoaded();
        }
        protected abstract void OnGameObjectLoaded();
    }
}