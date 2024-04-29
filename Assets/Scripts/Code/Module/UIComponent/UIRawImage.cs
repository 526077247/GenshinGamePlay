using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace TaoTie
{
    public class UIRawImage : UIBaseContainer,IOnCreate<string>,IOnDestroy
    {
        string spritePath;
        RawImage image;
        BgRawAutoFit bgRawAutoFit;
        long id;
        bool grayState;
        #region override

        public void OnCreate(string path)
        {
            id = IdGenerater.Instance.GenerateId();
            SetSpritePath(path).Coroutine();
        }

        public void OnDestroy()
        {
            if (!string.IsNullOrEmpty(spritePath))
            {
                this.image.texture = null;
                ImageLoaderManager.Instance?.ReleaseImage(spritePath);
                spritePath = null;
            }
        }
        #endregion
        
        void ActivatingComponent()
        {
            if (this.image == null)
            {
                this.image = this.GetGameObject().GetComponent<RawImage>();
                if (this.image == null)
                {
                    Log.Error($"添加UI侧组件UIRawImage时，物体{this.GetGameObject().name}上没有找到RawImage组件");
                }
                this.bgRawAutoFit =this.GetGameObject().GetComponent<BgRawAutoFit>();
            }
        }
        public async ETTask SetSpritePath(string sprite_path)
        {
            CoroutineLock coroutine = null;
            try
            {
                coroutine = await CoroutineLockManager.Instance.Wait(CoroutineLockType.UIImage, this.id);
                if (sprite_path == this.spritePath) return;
                this.ActivatingComponent();
                if (this.bgRawAutoFit != null) this.bgRawAutoFit.enabled = false;
                this.image.enabled = false;
                var base_sprite_path = this.spritePath;
                this.spritePath = sprite_path;
                if (string.IsNullOrEmpty(sprite_path))
                {
                    this.image.texture = null;
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
                    this.image.texture = sprite.texture;
                    if (this.bgRawAutoFit != null)
                    {
                        this.bgRawAutoFit.bgSprite = sprite.texture;
                        this.bgRawAutoFit.enabled = true;
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
            return this.spritePath;
        }

        public void SetImageColor(Color color)
        {
            this.ActivatingComponent();
            this.image.color = color;
        }
        public void SetImageAlpha(float a)
        {
            this.ActivatingComponent();
            this.image.color = new Color(this.image.color.r,this.image.color.g,
                this.image.color.b,a);
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

    }
}
