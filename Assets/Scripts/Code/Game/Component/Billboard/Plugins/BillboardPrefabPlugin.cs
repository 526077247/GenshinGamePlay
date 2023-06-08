using UnityEngine;

namespace TaoTie
{

    public abstract class BillboardPrefabPlugin<T>: BillboardPlugin<T> where T : ConfigBillboardPrefabPlugin
    {
        protected GameObject obj { get; private set; }
        
        protected override void InitInternal()
        {
            LoadObj().Coroutine();
        }

        
        protected override void DisposeInternal()
        {
            if (obj != null)
            {
                GameObjectPoolManager.Instance.RecycleGameObject(obj);
                obj = null;
            }
        }

        protected override void UpdateInternal()
        {
            var mainC = CameraManager.Instance.MainCamera();
            if (mainC != null && obj != null)
            {
                obj.transform.rotation = mainC.transform.rotation;
                obj.transform.position = target.position + billboardComponent.Config.Offset + config.Offset;
            }
            if (obj != null && obj.activeSelf!= billboardComponent.Enable)
            {
                obj.SetActive(billboardComponent.Enable);
            }
        }

        private async ETTask LoadObj()
        {
            if(string.IsNullOrWhiteSpace(config.PrefabPath)) return;
            var obj = await GameObjectPoolManager.Instance.GetGameObjectAsync(config.PrefabPath);
            if (billboardComponent.IsDispose)
            {
                GameObjectPoolManager.Instance.RecycleGameObject(obj);
                return;
            }

            this.obj = obj;
            obj.transform.position = target.position + billboardComponent.Config.Offset + config.Offset;
            var mainC = CameraManager.Instance.MainCamera();
            if (mainC != null && obj != null)
            {
                obj.transform.rotation = mainC.transform.rotation;
            }
            OnGameObjectLoaded();
        }
        protected abstract void OnGameObjectLoaded();
    }
}