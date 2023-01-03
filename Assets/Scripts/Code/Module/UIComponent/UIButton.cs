using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TaoTie
{
    public class UIButton : UIBaseContainer,IOnCreate,IOnDestroy
    {
        UnityAction onclick;
        Button button;
        bool grayState;
        Image image;
        string spritePath;

        #region override

        public void OnCreate()
        {
            grayState = false;
        }
        
        public void OnDestroy()
        {
            if (this.onclick != null)
                this.button.onClick.RemoveListener(this.onclick);
            if (!string.IsNullOrEmpty(this.spritePath))
                ImageLoaderManager.Instance?.ReleaseImage(this.spritePath);
            this.onclick = null;
        }
        #endregion
        void ActivatingComponent()
        {
            if (this.button == null)
            {
                this.button = this.GetGameObject().GetComponent<Button>();
                if (this.button == null)
                {
                    Log.Error($"添加UI侧组件UIButton时，物体{this.GetGameObject().name}上没有找到Button组件");
                }
            }
        }
        void ActivatingImageComponent()
        {
            if (this.image == null)
            {
                this.image = this.GetGameObject().GetComponent<Image>();
                if (this.image == null)
                {
                    Log.Error($"添加UI侧组件UIButton时，物体{this.GetGameObject().name}上没有找到Image组件");
                }
            }
        }
        public void SetOnClick(Action callback)
        {
            this.ActivatingComponent();
            this.RemoveOnClick();
            this.onclick = () =>
            {
                //SoundComponent.Instance.PlaySound("Audio/Common/Click.mp3");
                callback?.Invoke();
            };
            this.button.onClick.AddListener(this.onclick);
        }

        public void RemoveOnClick()
        {
            if (this.onclick != null)
                this.button.onClick.RemoveListener(this.onclick);
            this.onclick = null;
        }

        public void SetEnabled(bool flag)
        {
            this.ActivatingComponent();
            this.button.enabled = flag;
        }

        public void SetInteractable(bool flag)
        {
            this.ActivatingComponent();
            this.button.interactable = flag;
        }
        /// <summary>
        /// 设置按钮变灰
        /// </summary>
        /// <param name="isGray">是否变灰</param>
        /// <param name="includeText">是否包含字体, 不填的话默认为true</param>
        /// <param name="affectInteractable">是否影响交互, 不填的话默认为true</param>
        public async ETTask SetBtnGray(bool isGray, bool includeText = true, bool affectInteractable = true)
        {
            if (this.grayState == isGray) return;
            this.ActivatingImageComponent();
            this.grayState = isGray;
            var mat = await MaterialManager.Instance.LoadMaterialAsync("UI/UICommon/Materials/uigray.mat");
            if (affectInteractable)
            {
                this.image.raycastTarget = !isGray;
            }
            this.SetBtnGray(mat, isGray, includeText);
        }

        public void SetBtnGray(Material grayMaterial, bool isGray, bool includeText)
        {
            this.ActivatingImageComponent();
            GameObject go = this.GetGameObject();
            if (go == null)
            {
                return;
            }
            Material mt = null;
            if (isGray)
            {
                mt = grayMaterial;
            }
            var coms = go.GetComponentsInChildren<Image>(true);
            for (int i = 0; i < coms.Length; i++)
            {
                coms[i].material = mt;
            }

            if (includeText)
            {
                var textComs = go.GetComponentsInChildren<Text>();
                for (int i = 0; i < textComs.Length; i++)
                {
                    var uITextColorCtrl = TextColorCtrl.Get(textComs[i].gameObject);
                    if (isGray)
                    {
                        uITextColorCtrl.SetTextColor(new Color(89 / 255f, 93 / 255f, 93 / 255f));
                    }
                    else
                    {
                        uITextColorCtrl.ClearTextColor();
                    }
                }
            }
        }
        public async ETTask SetSpritePath(string sprite_path)
        {
            if (string.IsNullOrEmpty(sprite_path)) return;
            if (sprite_path == this.spritePath) return;
            this.ActivatingImageComponent();
            var base_sprite_path = this.spritePath;
            this.spritePath = sprite_path;
            var sprite = await ImageLoaderManager.Instance.LoadImageAsync(sprite_path);
            if (sprite == null)
            {
                ImageLoaderManager.Instance.ReleaseImage(sprite_path);
                return;
            }

            if (!string.IsNullOrEmpty(base_sprite_path))
                ImageLoaderManager.Instance.ReleaseImage(base_sprite_path);

            this.image.sprite = sprite;

        }

        public string GetSpritePath()
        {
            return this.spritePath;
        }

        public void SetImageColor(Color color)
        {
            this.ActivatingImageComponent();
            this.image.color = color;
        }
    }
}