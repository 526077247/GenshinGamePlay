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
        private long id;
        #region override

        public void OnCreate()
        {
            id = IdGenerater.Instance.GenerateId();
            grayState = false;
        }
        
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
            this.image.material = mt;
            if (affectInteractable)
            {
                this.image.raycastTarget = !this.grayState;
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
        public async ETTask SetSpritePath(string sprite_path,bool setNativeSize = false)
        {
            CoroutineLock coroutine = null;
            try
            {
                coroutine = await CoroutineLockManager.Instance.Wait(CoroutineLockType.UIImage, this.id);
                if (sprite_path == this.spritePath) return;
                this.ActivatingImageComponent();
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
            this.ActivatingImageComponent();
            this.image.color = color;
        }
        
        public void SetNativeSize()
        {
            this.ActivatingImageComponent();
            this.image.SetNativeSize();
        }
    }
}