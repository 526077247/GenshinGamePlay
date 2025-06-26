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
        string texturePath;
        RawImage image;
        BgRawAutoFit bgRawAutoFit;

        bool grayState;
        private int version;
        private string cacheUrl;
        private bool isSetTexture;
        
        private Texture2D base64texture;

        #region override

        public void OnCreate(string path)
        {
            SetTexturePath(path).Coroutine();
        }

        public void OnDestroy()
        {
            if (!string.IsNullOrEmpty(texturePath))
            {
                ImageLoaderManager.Instance?.ReleaseImage(texturePath);
                texturePath = null;
            }
            if (!string.IsNullOrEmpty(cacheUrl))
            {
                ImageLoaderManager.Instance?.ReleaseOnlineImage(cacheUrl);
                cacheUrl = null;
            }

            if (isSetTexture)
            {
                this.image.texture = null;
                isSetTexture = false;
                ClearBase64();
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
        public async ETTask SetTexturePath(string texturePath, bool setNativeSize = false)
        {
            version++;
            int thisVersion = version;
            if (texturePath == this.texturePath && !isSetTexture)
            {
                if (image != null) this.image.enabled = true;
                return;
            }

            this.ActivatingComponent();
            if (this.bgRawAutoFit != null) this.bgRawAutoFit.enabled = false;
            this.image.enabled = false;
            var baseTexturePath = this.texturePath;
           
            if (string.IsNullOrEmpty(texturePath))
            {
                this.image.texture = null;
                this.texturePath = texturePath;
                this.image.enabled = true;
                isSetTexture = false;
            }
            else
            {
                var texture = await ImageLoaderManager.Instance.LoadTextureAsync(texturePath);
                if (thisVersion != version)
                {
                    ImageLoaderManager.Instance.ReleaseImage(texturePath);
                    return;
                }
                this.texturePath = texturePath;
                this.image.enabled = true;
                this.image.texture = texture;
                isSetTexture = false;
                if(setNativeSize)
                    this.SetNativeSize();
                if (this.bgRawAutoFit != null)
                {
                    this.bgRawAutoFit.SetTexture(texture);
                    this.bgRawAutoFit.enabled = true;
                }
            }
            if(!string.IsNullOrEmpty(baseTexturePath))
                ImageLoaderManager.Instance.ReleaseImage(baseTexturePath);
           
        }
        
        /// <summary>
        /// 设置网络图片地址（注意尽量不要和SetTexturePath混用
        /// </summary>
        /// <param name="url"></param>
        /// <param name="defaultTexturePath"></param>
        public async ETTask SetOnlineTexturePath(string url, string defaultTexturePath = null)
        {
            this.ActivatingComponent();
            if (!string.IsNullOrEmpty(defaultTexturePath))
            {
                await SetTexturePath(defaultTexturePath);
            }
            version++;
            int thisVersion = version;
            var texture = await ImageLoaderManager.Instance.GetOnlineTexture(url);
            if (texture != null)
            {
                if (thisVersion != version)
                {
                    ImageLoaderManager.Instance.ReleaseOnlineImage(url);
                    return;
                }
                SetTexture(texture);
                if (!string.IsNullOrEmpty(cacheUrl))
                {
                    ImageLoaderManager.Instance.ReleaseOnlineImage(cacheUrl);
                    cacheUrl = null;
                }
                cacheUrl = url;
            }
        }

        public string GetTexturePath()
        {
            return this.texturePath;
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
        public void SetTexture(Texture texture)
        {
            this.ActivatingComponent();
            this.image.texture = texture;
            isSetTexture = true;
        }
        
        public void SetNativeSize()
        {
            this.image?.SetNativeSize();
        }
                
        public Texture GetTexture()
        {
            ActivatingComponent();
            return this.image.texture;
        }
        
        public void SetBase64(string data)
        {
            // 将Base64字符串转换为字节数组
            string base64Str = data;
            base64Str = base64Str.Replace("data:image/png;base64,", "").Replace("data:image/jgp;base64,", "")
                .Replace("data:image/jpg;base64,", "").Replace("data:image/jpeg;base64,", "");
            byte[] imageBytes = Convert.FromBase64String(base64Str);
            // 创建一个新的Texture2D对象
            if (base64texture == null) 
                base64texture = new Texture2D(2, 2); // 初始大小无所谓，因为后面会重新加载
            // 加载图像数据
            base64texture.LoadImage(imageBytes);
            SetTexture(base64texture);
        }

        private void ClearBase64()
        {
            if (base64texture != null)
            {
                GameObject.Destroy(base64texture);
                base64texture = null;
            }
        }
    }
}
