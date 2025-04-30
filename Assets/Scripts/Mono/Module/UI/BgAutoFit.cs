using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TaoTie
{
    [ExecuteAlways]
    [RequireComponent(typeof(Image))]
    public class BgAutoFit : MonoBehaviour
    {
        RectTransform rectTransform;
        Image bg;

        public Sprite bgSprite;
        
        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            bg = GetComponent<Image>();
        }

        void Start()
        {
            if (bgSprite == null)
                bgSprite = bg.sprite;
            else
                bg.sprite = bgSprite;
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
            var flag1 = screenW / texture.bounds.size.x;
            var flag2 = screenH / texture.bounds.size.y;
            if (flag1 < flag2)
                rectTransform.sizeDelta = new Vector2(flag2 * texture.bounds.size.x * signFlag, screenH * signFlag);
            else
                rectTransform.sizeDelta = new Vector2(screenW * signFlag, flag1 * texture.bounds.size.y * signFlag);
            
            var canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
            {
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

        public void SetSprite(Sprite newBgSprite)
        {
            bgSprite = newBgSprite;
            if (bgSprite == null)
                bgSprite = bg.sprite;
            else
                bg.sprite = bgSprite;
            Size();
        }
    }
}