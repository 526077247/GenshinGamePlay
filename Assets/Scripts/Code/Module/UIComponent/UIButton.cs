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
    public class UIButton : UIBaseContainer,IOnDestroy
    {
        UnityAction onclick;
        Button button;
        bool grayState;
        Image image;
        string spritePath;
        private int version = 0;
        
        #region override

        public void OnDestroy()
        {
            if (this.onclick != null)
                this.button.onClick.RemoveListener(this.onclick);
            if (!string.IsNullOrEmpty(this.spritePath))
            {
                this.image.sprite = null;
                ImageLoaderManager.Instance?.ReleaseImage(this.spritePath);
                spritePath = null;
            }
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
            this.grayState = isGray;
            this.ActivatingImageComponent();
            
            Material mt = null;
            if (isGray)
            {
                mt = await MaterialManager.Instance.LoadMaterialAsync("UI/UICommon/Materials/uigray.mat");
                if (!this.grayState)
                {
                    mt = null;
                }
            }

            if (this.image != null)
            {
                this.image.material = mt;
                if (affectInteractable)
                {
                    this.image.raycastTarget = !this.grayState;
                }
            }

            this.SetBtnGray(mt, this.grayState, includeText);
        }

        private void SetBtnGray(Material grayMaterial, bool isGray, bool includeText)
        {
            this.ActivatingImageComponent();
            GameObject go = this.GetGameObject();
            if (go == null)
            {
                return;
            }
            Material mt = grayMaterial;
            var coms = go.GetComponentsInChildren<Image>(true);
            for (int i = 0; i < coms.Length; i++)
            {
                coms[i].material = mt;
            }

            if (includeText)
            {
                var textComs = go.GetComponentsInChildren<Text>(true);
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
                var textComs2 = go.GetComponentsInChildren<TMPro.TMP_Text>(true);
                for (int i = 0; i < textComs2.Length; i++)
                {
                    var uITextColorCtrl = TextColorCtrl.Get(textComs2[i].gameObject);
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
        public async ETTask SetSpritePath(string spritePath,bool setNativeSize = false,Action callback = null)
        {
            version++;
            int thisVersion = version;
            if (spritePath == this.spritePath)
            {
                if (image != null) this.image.enabled = true;
                callback?.Invoke();
                return;
            }

            this.ActivatingImageComponent();
            this.image.enabled = false;
            var baseSpritePath = this.spritePath;

            if (string.IsNullOrEmpty(spritePath))
            {
                this.image.sprite = null;
                this.image.enabled = true;
                this.spritePath = spritePath;
            }
            else
            {
                var sprite = await ImageLoaderManager.Instance.LoadImageAsync(spritePath);
                if (thisVersion != version)
                {
                    ImageLoaderManager.Instance.ReleaseImage(spritePath);
                    callback?.Invoke();
                    return;
                }
                this.spritePath = spritePath;
                this.image.enabled = true;
                this.image.sprite = sprite;

                if(setNativeSize)
                    this.SetNativeSize();
            }
            if(!string.IsNullOrEmpty(baseSpritePath))
                ImageLoaderManager.Instance.ReleaseImage(baseSpritePath);
           
            callback?.Invoke();
        }
        public string GetSpritePath()
        {
            return this.spritePath;
        }

        public void SetImageColor(string colorStr)
        {
            if(string.IsNullOrEmpty(colorStr)) return;
            if (!colorStr.StartsWith("#")) colorStr = "#" + colorStr;
            if (ColorUtility.TryParseHtmlString(colorStr, out var color))
            {
                this.ActivatingImageComponent();
                this.image.color = color;
            }
            else
            {
                Log.Error("Set image color error, color is "+colorStr);
            }
        }
        public void SetImageColor(Color color)
        {
            this.ActivatingImageComponent();
            
        }
        public void SetNativeSize()
        {
            this.ActivatingImageComponent();
            this.image.SetNativeSize();
        }
        
        public void SetFillAmount(float value)
        {
            this.ActivatingImageComponent();
            this.image.fillAmount = value;
        }

        public void SetImageAlpha(float a,bool changeChild=false)
        {
            this.ActivatingImageComponent();
            this.image.color = new Color(this.image.color.r,this.image.color.g,
                this.image.color.b,a);
            if (changeChild)
            {
                var images = this.image.GetComponentsInChildren<Image>(false);
                for (int i = 0; i < images.Length; i++)
                {
                    images[i].color = new Color(images[i].color.r,images[i].color.g, images[i].color.b,a);
                }
                // var texts = this.unity_uiimage.GetComponentsInChildren<TMPro.TMP_Text>(false);
                // for (int i = 0; i < texts.Length; i++)
                // {
                //     texts[i].color = new Color(texts[i].color.r,texts[i].color.g, texts[i].color.b,a);
                // }
            }
        }
    }
}