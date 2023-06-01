using UnityEngine;

namespace TaoTie
{
    public class BillboardNamePlugin: BillboardPlugin<ConfigBillboardNamePlugin>
    {
        private TextMesh font;
        private GameObject obj;
        #region BillboardPlugin

        protected override void InitInternal()
        {
            InitInternalAsync().Coroutine();
        }
        private async ETTask InitInternalAsync()
        {
            var goh = billboardComponent.GetParent<Entity>().GetComponent<GameObjectHolderComponent>();
            await goh.WaitLoadGameObjectOver();
            if(goh.IsDispose || billboardComponent.IsDispose) return;
            
            var pointer = goh.GetCollectorObj<Transform>(billboardComponent.Config.AttachPoint);
            if (pointer == null)
            {
                GameObjectPoolManager.Instance.RecycleGameObject(obj);
                return;
            }
            
            obj = new GameObject("Name");
            obj.transform.localScale = 0.1f * Vector3.one;
            obj.transform.SetParent(pointer);
            obj.transform.localPosition = billboardComponent.Config.Offset + config.Offset;
            var mainC = CameraManager.Instance.MainCamera();
            if (mainC != null && obj != null)
            {
                obj.transform.rotation = mainC.transform.rotation;
                obj.transform.localPosition = obj.transform.localRotation*(billboardComponent.Config.Offset + config.Offset);
            }
            font = obj.AddComponent<TextMesh>();
            font.fontSize = 36;
            font.alignment = TextAlignment.Center;
            font.anchor = TextAnchor.MiddleCenter;
            font.color = config.BaseColor;//todo:受其他影响
            SetName();
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

        protected override void DisposeInternal()
        {
            font = null;
            if (obj != null)
            {
                Object.Destroy(obj);
                obj = null;
            }
        }
        
        #endregion

        private void SetName()
        {
            if (config.ShowUnitName)
            {
                var unit = billboardComponent.GetParent<Unit>();
                if (unit != null && !string.IsNullOrEmpty(unit.Config.Name))
                {
                    font.text = I18NManager.Instance.I18NGetText(unit.Config.Name);
                    return;
                }
            }
            font.text = I18NManager.Instance.I18NGetText(config.NameI18NKey);
        }
    }
}