using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace TaoTie
{
    /// <summary>
    /// UI弹窗后面的背景截图
    /// </summary>
    [RequireComponent(typeof(RawImage))]
    [ExecuteAlways]
    public class BackgroundBlur : MonoBehaviour
    {
        public Material blurMaterial;
        public RawImage rImage;

        private static Texture2D screenShotTemp;
        public static int RefCount = 0;//引用次数
#if UNITY_EDITOR
        private void Awake()
        {
            this.blurMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/AssetsPackage/UI/UICommon/Materials/uitexblur.mat");
            rImage = this.GetComponent<RawImage>();
        }
#endif
        
        private void OnEnable()
        {
            StartCoroutine(Snapshoot());
        }

        private IEnumerator Snapshoot()
        {
            if (rImage == null)
            {
                Log.Warning("Background Blur is warring !!!");
                yield break;
            }
            yield return ReadPixels();
            
        }

        private IEnumerator ReadPixels()
        {
            var mainCamera = Camera.main;
            if (mainCamera == null) yield break;
            rImage.enabled = false;
            while (RefCount>0 && screenShotTemp == null)
            {
                yield return new WaitForEndOfFrame();
            }
            RefCount++;
            if (screenShotTemp == null)
            {
                var uiLayer = LayerMask.NameToLayer("UI");
                var cameras = FindObjectsOfType<Camera>(true);
                Camera uiCamera = null;
                for (int i = 0; i < cameras.Length; i++)
                {
                    if (cameras[i].gameObject.layer == uiLayer)
                    {
                        uiCamera = cameras[i];
                        break;
                    }
                }
                if(uiCamera!=null) uiCamera.enabled = false;
                yield return new WaitForEndOfFrame();
                if (RefCount > 0)//防止等一帧回来已经被关了
                {
                    // 先创建一个的空纹理，大小可根据实现需要来设置  
                    var rect = new Rect(0, 0, Screen.width, Screen.height);
                    screenShotTemp = new Texture2D((int) rect.width, (int) rect.height, TextureFormat.RGB24, false);
                    // 读取屏幕像素信息并存储为纹理数据，  
                    screenShotTemp.ReadPixels(rect, 0, 0);
                    screenShotTemp.Apply();
                    // 调用shader模糊
                    if (blurMaterial != null)
                    {
                        RenderTexture destination = RenderTexture.GetTemporary((int) rect.width, (int) rect.height, 0,RenderTextureFormat.Default,RenderTextureReadWrite.Linear);
                        blurMaterial.SetTexture("_MainTex",screenShotTemp);
                        Graphics.Blit(null, destination, blurMaterial);

                        var prevRT = RenderTexture.active;
                        RenderTexture.active = destination;
                        screenShotTemp.ReadPixels(rect, 0, 0);
                        screenShotTemp.Apply();
                        RenderTexture.active = prevRT;
                        destination.Release();
                    }
                }
                if(uiCamera!=null) uiCamera.enabled = true;
            }
            
            rImage.enabled = true;
            rImage.texture = screenShotTemp;
        }

        private void OnDisable()
        {
            RefCount--;
            if (RefCount <= 0)
            {
                RefCount = 0;
                if (screenShotTemp != null)
                {
                    Destroy(screenShotTemp);
                    screenShotTemp = null;
                }
            }
        }
    }
}
