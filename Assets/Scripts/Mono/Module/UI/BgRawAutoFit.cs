using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TaoTie
{
    [ExecuteAlways]
    [RequireComponent(typeof(RawImage))]
    public class BgRawAutoFit : MonoBehaviour
    {
        RectTransform rectTransform;
        RawImage bg;

        public Texture bgSprite;
        
        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            bg = GetComponent<RawImage>();
        }

        void Start()
        {
            if (bgSprite == null)
                bgSprite = bg.texture;
            else
                bg.texture = bgSprite;
            Size();
        }

        void Size()
        {
            if (bgSprite == null) return;
            //屏幕缩放比
            var screenH = Screen.height;
            var screenW = Screen.width;
            var flagx = Define.DesignScreenWidth / Define.DesignScreenHeight;
            var flagy = (float) screenW / screenH;
            var signFlag = flagx > flagy
                ? Define.DesignScreenWidth / screenW
                : Define.DesignScreenHeight / screenH;
            //图片缩放比
            var texture = bgSprite;
            var flag1 = (float) screenW / texture.width;
            var flag2 = (float) screenH / texture.height;
            if (flag1 < flag2)
                rectTransform.sizeDelta = new Vector2(flag2 * texture.width * signFlag, screenH * signFlag);
            else
                rectTransform.sizeDelta = new Vector2(screenW * signFlag, flag1 * texture.height * signFlag);
            var canvas = GetComponentInParent<Canvas>();
            if (Application.isPlaying && canvas != null)
            {
#if UNITY_EDITOR
                var type = UnityEditor.PrefabUtility.GetPrefabAssetType(gameObject);
                var status = UnityEditor.PrefabUtility.GetPrefabInstanceStatus(gameObject);
                // 是否为预制体实例判断
                if (type != UnityEditor.PrefabAssetType.NotAPrefab && status != UnityEditor.PrefabInstanceStatus.NotAPrefab)
                {
                    return;
                }
#endif
                var parent = transform.parent;
                var siblingIndex = transform.GetSiblingIndex();
                transform.SetParent(canvas.transform);
                rectTransform.localPosition = Vector3.zero;
                rectTransform.anchoredPosition = Vector2.zero;
                transform.SetParent(parent, true);
                transform.SetSiblingIndex(siblingIndex);
            }
            else
            {
                rectTransform.localPosition = Vector3.zero;
                rectTransform.anchoredPosition = Vector2.zero;
            }
        }

        public void SetSprite(Texture newBgSprite)
        {
            bgSprite = newBgSprite;
            if (bgSprite == null)
                bgSprite = bg.texture;
            else
                bg.texture = bgSprite;
            Size();
        }
    }
}