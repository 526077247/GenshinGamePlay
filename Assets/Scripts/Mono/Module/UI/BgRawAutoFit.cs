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