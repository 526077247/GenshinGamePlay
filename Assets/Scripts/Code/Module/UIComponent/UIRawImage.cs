using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace TaoTie
{
    public class UIRawImage : UIBaseContainer,IOnCreate<string>
    {
        string sprite_path;
        RawImage unity_uiimage;
        private BgRawAutoFit BgRawAutoFit;
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
                this.unity_uiimage = this.GetGameObject().GetComponent<RawImage>();
                if (this.unity_uiimage == null)
                {
                    Log.Error($"添加UI侧组件UIRawImage时，物体{this.GetGameObject().name}上没有找到RawImage组件");
                }
                this.BgRawAutoFit =this.GetGameObject().GetComponent<BgRawAutoFit>();
            }
        }
        public async ETTask SetSpritePath(string sprite_path)
        {
            CoroutineLock coroutine = null;
            try
            {
                coroutine = await CoroutineLockManager.Instance.Wait(CoroutineLockType.UIImage, this.Id);
                if (sprite_path == this.sprite_path) return;
                this.ActivatingComponent();
                if (this.BgRawAutoFit != null) this.BgRawAutoFit.enabled = false;
                this.unity_uiimage.enabled = false;
                var base_sprite_path = this.sprite_path;
                this.sprite_path = sprite_path;
                if (string.IsNullOrEmpty(sprite_path))
                {
                    this.unity_uiimage.texture = null;
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
                    this.unity_uiimage.texture = sprite.texture;
                    if (this.BgRawAutoFit != null)
                    {
                        this.BgRawAutoFit.bgSprite = sprite.texture;
                        this.BgRawAutoFit.enabled = true;
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

        public string GetSpritePath()
        {
            return this.sprite_path;
        }

        public void SetImageColor(Color color)
        {
            this.ActivatingComponent();
            this.unity_uiimage.color = color;
        }
        public void SetImageAlpha(float a)
        {
            this.ActivatingComponent();
            this.unity_uiimage.color = new Color(this.unity_uiimage.color.r,this.unity_uiimage.color.g,
                this.unity_uiimage.color.b,a);
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

    }
}
