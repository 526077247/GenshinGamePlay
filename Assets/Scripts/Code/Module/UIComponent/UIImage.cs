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
        private long id;
        #region override

        public void OnCreate(string path)
        {
            id = IdGenerater.Instance.GenerateId();
            SetSpritePath(path).Coroutine();
        }

        public void OnDestroy()
        {
            if (string.IsNullOrEmpty(spritePath))
            {
                ImageLoaderManager.Instance?.ReleaseImage(spritePath);
                spritePath = null;
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
        public async ETTask SetSpritePath(string sprite_path,bool setNativeSize = false)
        {
            CoroutineLock coroutine = null;
            try
            {
                coroutine = await CoroutineLockManager.Instance.Wait(CoroutineLockType.UIImage, this.id);
                if (sprite_path == this.spritePath) return;
                this.ActivatingComponent();
                if (this.bgAutoFit != null) this.bgAutoFit.enabled = false;
                this.image.enabled = false;
                var base_sprite_path = this.spritePath;
                this.spritePath = sprite_path;
                if (string.IsNullOrEmpty(sprite_path))
                {
                    this.image.sprite = null;
                    this.image.enabled = true;
                }
                else
                {
                    var sprite = await ImageLoaderManager.Instance.LoadImageAsync(sprite_path);
                    this.image.enabled = true;
                    if (sprite == null)
                    {
                        ImageLoaderManager.Instance.ReleaseImage(sprite_path);
                        return;
                    }
                    this.image.sprite = sprite;
                    if(setNativeSize)
                        this.SetNativeSize();
                    if (this.bgAutoFit != null)
                    {
                        this.bgAutoFit.bgSprite = sprite;
                        this.bgAutoFit.enabled = true;
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
            this.image.SetNativeSize();
        }

        public string GetSpritePath()
        {
            return this.spritePath;
        }
        public void SetColor(string colorStr)
        {
            if (!colorStr.StartsWith("#")) colorStr = "#" + colorStr;
            if (ColorUtility.TryParseHtmlString(colorStr, out var color))
            {
                this.ActivatingComponent();
                this.image.color = color;
            }
            else
            {
                Log.Info(colorStr);
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
            this.ActivatingComponent();
            Material mt = null;
            if (isGray)
            {
                mt = await MaterialManager.Instance.LoadMaterialAsync("UI/UICommon/Materials/uigray.mat");
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
    }
}
