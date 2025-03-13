using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;
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
                yield return null;
            }
            else
            {
                yield return ReadPixels();
            }
            
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
                GameObject uiCamera = null;
                var cd = mainCamera.GetUniversalAdditionalCameraData();
                for (int i = 0; i < cd.cameraStack.Count; i++)
                {
                    if (cd.cameraStack[i].gameObject.layer == LayerMask.NameToLayer("UI"))
                    {
                        uiCamera = cd.cameraStack[i].gameObject;
                        break;
                    }
                }
                uiCamera?.SetActive(false);
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

                        screenShotTemp.ReadPixels(rect, 0, 0);
                        screenShotTemp.Apply();
                        destination.Release();
                    }
                }
                uiCamera?.SetActive(true);
            }
            
            rImage.enabled = true;
            rImage.texture = screenShotTemp;
        }

        private void OnDisable()
        {
            RefCount--;
            if (RefCount <= 0)
            {
                if (screenShotTemp != null)
                    Destroy(screenShotTemp);
            }
        }
    }
}
