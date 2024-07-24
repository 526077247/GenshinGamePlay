using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace TaoTie
{
    public class UIImage:UIBaseContainer,IOnCreate<string>,IOnDestroy
    {
        string spritePath;
        Image image;
        BgAutoFit bgAutoFit;
        bool grayState;
        private bool isSetSprite;
        private int version = 0;
        private string cacheUrl;
        #region override

        public void OnCreate(string path)
        {
            SetSpritePath(path).Coroutine();
        }

        public void OnDestroy()
        {
            if (!string.IsNullOrEmpty(spritePath))
            {
                this.image.sprite = null;
                ImageLoaderManager.Instance?.ReleaseImage(spritePath);
                spritePath = null;
            }

            if (isSetSprite)
            {
                this.image.sprite = null;
                isSetSprite = false;
            }
            
            if (!string.IsNullOrEmpty(cacheUrl))
            {
                ImageLoaderManager.Instance?.ReleaseOnlineImage(cacheUrl);
            }
        }

        #endregion
        
        void ActivatingComponent()
        {
            if (this.image == null)
            {
                this.image = this.GetGameObject().GetComponent<Image>();
                if (this.image == null)
                {
                    Log.Error($"添加UI侧组件UIImage时，物体{this.GetGameObject().name}上没有找到Image组件");
                }
                this.bgAutoFit =  this.GetGameObject().GetComponent<BgAutoFit>();
            }
        }
        /// <summary>
        /// 设置图片地址（注意尽量不要和SetOnlineSpritePath混用
        /// </summary>
        /// <param name="spritePath"></param>
        /// <param name="setNativeSize"></param>
        /// <param name="callback"></param>
        public async ETTask SetSpritePath(string spritePath,bool setNativeSize = false,Action callback = null)
        {
            version++;
            int thisVersion = version;
            if (spritePath == this.spritePath && !isSetSprite)
            {
                this.image.enabled = true;
                callback?.Invoke();
                return;
            }
            this.ActivatingComponent();
            if (this.bgAutoFit != null) this.bgAutoFit.enabled = false;
            this.image.enabled = false;
            var baseSpritePath = this.spritePath;

            if (string.IsNullOrEmpty(spritePath))
            {
                this.image.sprite = null;
                this.image.enabled = true;
                isSetSprite = false;
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
                isSetSprite = false;
                if(setNativeSize)
                    this.SetNativeSize();
                if (this.bgAutoFit != null)
                {
                    this.bgAutoFit.SetSprite(sprite);
                    this.bgAutoFit.enabled = true;
                }
            }
            if(!string.IsNullOrEmpty(baseSpritePath))
                ImageLoaderManager.Instance.ReleaseImage(baseSpritePath);
           
            callback?.Invoke();
        }

        /// <summary>
        /// 设置网络图片地址（注意尽量不要和SetSpritePath混用
        /// </summary>
        /// <param name="url"></param>
        /// <param name="setNativeSize"></param>
        /// <param name="defaultSpritePath"></param>
        public async ETTask SetOnlineSpritePath(string url, bool setNativeSize = false, string defaultSpritePath = null)
        {
            this.ActivatingComponent();
            if (!string.IsNullOrEmpty(defaultSpritePath))
            {
                await SetSpritePath(defaultSpritePath,setNativeSize);
            }

            var sprite = await ImageLoaderManager.Instance.GetOnlineSprite(url);
            if (sprite != null)
            {
                SetSprite(sprite);
                if (!string.IsNullOrEmpty(cacheUrl))
                {
                    ImageLoaderManager.Instance.ReleaseOnlineImage(cacheUrl);
                    cacheUrl = null;
                }
                cacheUrl = url;
            }
        }

        public void SetNativeSize()
        {
            this.image.SetNativeSize();
        }

        public string GetSpritePath()
        {
            return this.spritePath;
        }
        public void SetColor(string colorStr)
        {
            if(string.IsNullOrEmpty(colorStr)) return;
            if (!colorStr.StartsWith("#")) colorStr = "#" + colorStr;
            if (ColorUtility.TryParseHtmlString(colorStr, out var color))
            {
                this.ActivatingComponent();
                this.image.color = color;
            }
            else
            {
                Log.Error("Set image color error, color is "+colorStr);
            }
        }
        public void SetImageColor(Color color)
        {
            this.ActivatingComponent();
            this.image.color = color;
        }

        public Color GetImageColor()
        {
            this.ActivatingComponent();
            return this.image.color;
        }
        public void SetImageAlpha(float a,bool changeChild=false)
        {
            this.ActivatingComponent();
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
        public void SetEnabled(bool flag)
        {
            this.ActivatingComponent();
            this.image.enabled = flag;
        }
        public async ETTask SetImageGray(bool isGray)
        {
            if (this.grayState == isGray) return;
            this.ActivatingComponent();
            this.grayState = isGray;
            Material mt = null;
            if (isGray)
            {
                mt = await MaterialManager.Instance.LoadMaterialAsync("UI/UICommon/Materials/uigray.mat");
                if (!this.grayState)
                {
                    mt = null;
                }
            }
            this.image.material = mt;
        }
        public void SetFillAmount(float value)
        {
            this.ActivatingComponent();
            this.image.fillAmount = value;
        }
        // public void DoSetFillAmount(float newValue, float duration)
        // {
        //     this.ActivatingComponent();
        //     DOTween.To(() => this.unity_uiimage.fillAmount,x=> this.unity_uiimage.fillAmount=x, newValue, duration);
        // }

        public Material GetMaterial()
        {
            this.ActivatingComponent();
            return this.image.material;
        }

        public void SetSprite(Sprite sprite)
        {
            this.ActivatingComponent();
            this.image.sprite = sprite;
            isSetSprite = true;
        }
    }
}
