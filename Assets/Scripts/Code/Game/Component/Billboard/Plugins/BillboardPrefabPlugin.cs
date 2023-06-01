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
                obj.transform.localPosition = obj.transform.localRotation*(billboardComponent.Config.Offset + config.Offset);
            }
            if (obj != null && obj.activeSelf!= billboardComponent.Enable)
            {
                obj.SetActive(billboardComponent.Enable);
            }
        }

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
            var mainC = CameraManager.Instance.MainCamera();
            if (mainC != null && obj != null)
            {
                obj.transform.rotation = mainC.transform.rotation;
                obj.transform.localPosition = obj.transform.localRotation*(billboardComponent.Config.Offset + config.Offset);
            }
            OnGameObjectLoaded();
        }
        protected abstract void OnGameObjectLoaded();
    }
}