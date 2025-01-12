using UnityEngine;
using UnityEngine.Rendering;

namespace TaoTie
{
    public class BillboardNamePlugin: BillboardPlugin<ConfigBillboardNamePlugin>
    {
        private TextMesh font;
        private GameObject obj;
        #region BillboardPlugin

        protected override void InitInternal()
        {
            obj = new GameObject("Name");
            obj.transform.position = target.position + (billboardComponent.Config.Offset + config.Offset) * billboardComponent.Scale;
            obj.transform.localScale = Vector3.one * (billboardComponent.Scale * 0.1f);
            var mainC = CameraManager.Instance.MainCamera();
            if (mainC != null && obj != null)
            {
                obj.transform.rotation = mainC.transform.rotation;
            }
            font = obj.AddComponent<TextMesh>();
            font.fontSize = 20;
            font.alignment = TextAlignment.Center;
            font.anchor = TextAnchor.MiddleCenter;
            font.color = config.BaseColor;//todo:受其他影响
            var mesh = obj.GetComponent<MeshRenderer>();
            mesh.shadowCastingMode = ShadowCastingMode.Off;
            mesh.lightProbeUsage = LightProbeUsage.Off;
            SetName();
        }

        protected override void UpdateInternal()
        {
            var mainC = CameraManager.Instance.MainCamera();
            if (mainC != null && obj != null)
            {
                obj.transform.rotation = mainC.transform.rotation;
                obj.transform.position = target.position + (billboardComponent.Config.Offset + config.Offset)* billboardComponent.Scale;
                obj.transform.localScale = Vector3.one * (billboardComponent.Scale * 0.1f);
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