using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TaoTie
{
    [ExecuteAlways]
    public class BgAutoMax : MonoBehaviour
    {
        RectTransform rectTransform;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        void Start()
        {
            
            Size();
        }

        void Size()
        {
            //屏幕缩放比
            var screenH = Screen.height;
            var screenW = Screen.width;
            var flagx = Define.DesignScreenWidth / Define.DesignScreenHeight;
            var flagy = (float) screenW / screenH;
            var signFlag = flagx > flagy
                ? Define.DesignScreenWidth / screenW
                : Define.DesignScreenHeight / screenH;
            
            rectTransform.sizeDelta = new Vector2(screenW * signFlag, screenH * signFlag);

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
    }
}