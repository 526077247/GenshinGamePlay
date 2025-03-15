using UnityEngine;
using UnityEngine.Rendering;

namespace TaoTie
{
    public class BillboardHpPlugin: BillboardPlugin<ConfigBillboardHpPlugin>
    {
        private GameObject obj;
        private Vector3[] vertices;
        private Color[] colors;
        private int[] triangles;
        private MeshFilter filter;
        float x = 0.5f, y = 0.02f, z = 0f;
        
        private NumericComponent numericComponent;
        #region BillboardPlugin

        protected override void InitInternal()
        {
            numericComponent = billboardComponent.GetParent<Entity>().GetComponent<NumericComponent>();
            Messager.Instance.AddListener<NumericChange>(billboardComponent.Id,MessageId.NumericChangeEvt, OnNumericChange);
            x = 0.01f * config.Size.x;
            y = 0.01f * config.Size.y;
            obj = new GameObject("Hp");
            obj.transform.localScale = Vector3.one * billboardComponent.Scale;
            var mainC = CameraManager.Instance.MainCamera();
            if (mainC != null && obj != null)
            {
                obj.transform.rotation = mainC.transform.rotation;
            }
            filter = obj.AddComponent<MeshFilter>();
            var rend = obj.AddComponent<MeshRenderer>();
            vertices = new Vector3[8];
            colors = new Color[8];
            triangles = new int[12];

            vertices[0].Set(-x, y, z);
            vertices[2].Set(-x, -y, z);
            vertices[5].Set(x, y, z);
            vertices[7].Set(x, -y, z);
            Refresh();

            for (int j = 0; j < 2; j++)
            {
                for (int i = 0; i < 4; i++)
                {
                    colors[4 * j + i] = j == 1 ? config.BgColor : config.BleedColor;
                }
            }

            int currentCount = 0;
            for (int i = 0; i < 8; i = i + 4)
            {
                triangles[currentCount++] = i;
                triangles[currentCount++] = i + 3;
                triangles[currentCount++] = i + 1;

                triangles[currentCount++] = i;
                triangles[currentCount++] = i + 2;
                triangles[currentCount++] = i + 3;
            }

            filter.mesh.vertices = vertices;
            filter.mesh.triangles = triangles;
            filter.mesh.colors = colors;

            rend.material = MaterialManager.Instance.GetFromCache("Unit/Common/Materials/ProgressBar.mat");
            rend.shadowCastingMode = ShadowCastingMode.Off;
            rend.receiveShadows = false;
            rend.lightProbeUsage = LightProbeUsage.Off;
            rend.reflectionProbeUsage = ReflectionProbeUsage.Off;
        }

        protected override void UpdateInternal()
        {
            var mainC = CameraManager.Instance.MainCamera();
            if (mainC != null && obj != null && target!=null)
            {
                obj.transform.rotation = mainC.transform.rotation;
                obj.transform.position = target.position + (billboardComponent.Offset + config.Offset)* billboardComponent.Scale;
                obj.transform.localScale = Vector3.one * billboardComponent.Scale;
            }

            if (obj != null && obj.activeSelf!= billboardComponent.Enable)
            {
                obj.SetActive(billboardComponent.Enable);
            }
        }

        protected override void DisposeInternal()
        {
            Messager.Instance.RemoveListener<NumericChange>(billboardComponent.Id,MessageId.NumericChangeEvt, OnNumericChange);
            if (obj != null)
            {
                Object.Destroy(obj);
                obj = null;
            }
            vertices = null;
            colors = null;
            triangles = null;
            filter = null;
        }

        #endregion
        
        private void UpdateHud(float v)
        {
            float val = Mathf.Lerp(-x, x, v);
            vertices[1].Set(val, y, z);
            vertices[3].Set(val, -y, z);
            vertices[4] = vertices[1];
            vertices[6] = vertices[3];
            filter.mesh.vertices = vertices;
        }
        
        private void OnNumericChange(NumericChange evt)
        {
            if (evt.NumericType == NumericType.Hp|| evt.NumericType == NumericType.MaxHp)
            {
                Refresh();
            }
        }

        private void Refresh()
        {
            var progress = numericComponent.GetAsFloat(NumericType.Hp) / numericComponent.GetAsFloat(NumericType.MaxHp);
            UpdateHud(progress);
        }
    }
}