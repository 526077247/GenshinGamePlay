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
        string sprite_path;
        Image unity_uiimage;
        BgAutoFit BgAutoFit;
        private long Id;
        #region override

        public void OnCreate(string path)
        {
            Id = IdGenerater.Instance.GenerateId();
            SetSpritePath(path).Coroutine();
        }

        public void OnDestroy()
        {
            if (string.IsNullOrEmpty(sprite_path))
            {
                ImageLoaderManager.Instance?.ReleaseImage(sprite_path);
                sprite_path = null;
            }
        }

        #endregion
        
        void ActivatingComponent()
        {
            if (this.unity_uiimage == null)
            {
                this.unity_uiimage = this.GetGameObject().GetComponent<Image>();
                if (this.unity_uiimage == null)
                {
                    Log.Error($"添加UI侧组件UIImage时，物体{this.GetGameObject().name}上没有找到Image组件");
                }
                this.BgAutoFit =  this.GetGameObject().GetComponent<BgAutoFit>();
            }
        }
        public async ETTask SetSpritePath(string sprite_path,bool setNativeSize = false)
        {
            CoroutineLock coroutine = null;
            try
            {
                coroutine = await CoroutineLockManager.Instance.Wait(CoroutineLockType.UIImage, this.Id);
                if (sprite_path == this.sprite_path) return;
                this.ActivatingComponent();
                if (this.BgAutoFit != null) this.BgAutoFit.enabled = false;
                this.unity_uiimage.enabled = false;
                var base_sprite_path = this.sprite_path;
                this.sprite_path = sprite_path;
                if (string.IsNullOrEmpty(sprite_path))
                {
                    this.unity_uiimage.sprite = null;
                    this.unity_uiimage.enabled = true;
                }
                else
                {
                    var sprite = await ImageLoaderManager.Instance.LoadImageAsync(sprite_path);
                    this.unity_uiimage.enabled = true;
                    if (sprite == null)
                    {
                        ImageLoaderManager.Instance.ReleaseImage(sprite_path);
                        return;
                    }
                    this.unity_uiimage.sprite = sprite;
                    if(setNativeSize)
                        this.SetNativeSize();
                    if (this.BgAutoFit != null)
                    {
                        this.BgAutoFit.bgSprite = sprite;
                        this.BgAutoFit.enabled = true;
                    }
                }
                if(!string.IsNullOrEmpty(base_sprite_path))
                    ImageLoaderManager.Instance.ReleaseImage(base_sprite_path);
            }
            finally
            {
                coroutine?.Dispose();
            }
        }

        public void SetNativeSize()
        {
            this.unity_uiimage.SetNativeSize();
        }

        public string GetSpritePath()
        {
            return this.sprite_path;
        }
        public void SetColor(string colorStr)
        {
            if (!colorStr.StartsWith("#")) colorStr = "#" + colorStr;
            if (ColorUtility.TryParseHtmlString(colorStr, out var color))
            {
                this.ActivatingComponent();
                this.unity_uiimage.color = color;
            }
            else
            {
                Log.Info(colorStr);
            }
        }
        public void SetImageColor(Color color)
        {
            this.ActivatingComponent();
            this.unity_uiimage.color = color;
        }

        public Color GetImageColor()
        {
            this.ActivatingComponent();
            return this.unity_uiimage.color;
        }
        public void SetImageAlpha(float a,bool changeChild=false)
        {
            this.ActivatingComponent();
            this.unity_uiimage.color = new Color(this.unity_uiimage.color.r,this.unity_uiimage.color.g,
                this.unity_uiimage.color.b,a);
            if (changeChild)
            {
                var images = this.unity_uiimage.GetComponentsInChildren<Image>(false);
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
            this.unity_uiimage.enabled = flag;
        }
        public async ETTask SetImageGray(bool isGray)
        {
            this.ActivatingComponent();
            Material mt = null;
            if (isGray)
            {
                mt = await MaterialManager.Instance.LoadMaterialAsync("UI/UICommon/Materials/uigray.mat");
            }
            this.unity_uiimage.material = mt;
        }
        public void SetFillAmount(float value)
        {
            this.ActivatingComponent();
            this.unity_uiimage.fillAmount = value;
        }
        // public void DoSetFillAmount(float newValue, float duration)
        // {
        //     this.ActivatingComponent();
        //     DOTween.To(() => this.unity_uiimage.fillAmount,x=> this.unity_uiimage.fillAmount=x, newValue, duration);
        // }

        public Material GetMaterial()
        {
            this.ActivatingComponent();
            return this.unity_uiimage.material;
        }
    }
}
