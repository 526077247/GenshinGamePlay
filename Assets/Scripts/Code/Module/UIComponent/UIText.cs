using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace TaoTie
{
    public class UIText : UIBaseContainer,II18N
    {
        public Text unity_uitext;
        public I18NText unity_i18ncomp_touched;
        public string __text_key;
        public object[] keyParams;
        
        void ActivatingComponent()
        {
            if (this.unity_uitext == null)
            {
                this.unity_uitext = this.GetGameObject().GetComponent<Text>();
                if (this.unity_uitext == null)
                {
                    this.unity_uitext = this.GetGameObject().AddComponent<Text>();
                    Log.Info($"添加UI侧组件UIText时，物体{this.GetGameObject().name}上没有找到Text组件");
                }
                this.unity_i18ncomp_touched = this.GetGameObject().GetComponent<I18NText>();
            }
        }

        //当手动修改text的时候，需要将mono的i18textcomponent给禁用掉
        void __DisableI18Component(bool enable = false)
        {
            this.ActivatingComponent();
            if (this.unity_i18ncomp_touched != null)
            {
                this.unity_i18ncomp_touched.enabled = enable;
                if (!enable)
                    Log.Warning($"组件{this.GetGameObject().name}, text在逻辑层进行了修改，所以应该去掉去预设里面的I18N组件，否则会被覆盖");
            }
        }

        public string GetText()
        {
            this.ActivatingComponent();
            return this.unity_uitext.text;
        }

        public void SetText( string text)
        {
            this.__DisableI18Component();
            this.__text_key = null;
            this.unity_uitext.text = text;
        }
        public void SetI18NKey( string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                this.SetText("");
                return;
            }
            this.__DisableI18Component();
            this.__text_key = key;
            this.SetI18NText(null);
        }
        public void SetI18NKey( string key, params object[] paras)
        {
            if (string.IsNullOrEmpty(key))
            {
                this.SetText("");
                return;
            }
            this.__DisableI18Component();
            this.__text_key = key;
            this.SetI18NText(paras);
        }

        public void SetI18NText( params object[] paras)
        {
            if (string.IsNullOrEmpty(this.__text_key))
            {
                Log.Error("there is not key ");
            }
            else
            {
                this.__DisableI18Component();
                this.keyParams = paras;
                if (I18NManager.Instance.I18NTryGetText(this.__text_key, out var text) && paras != null)
                    text = string.Format(text, paras);
                this.unity_uitext.text = text;
            }
        }

        public void OnLanguageChange()
        {
            this.ActivatingComponent();
            {
                if (this.__text_key != null)
                {
                    if (I18NManager.Instance.I18NTryGetText(this.__text_key, out var text) && this.keyParams != null)
                        text = string.Format(text, this.keyParams);
                    this.unity_uitext.text = text;
                }
            }
        }

        public void SetTextColor( Color color)
        {
            this.ActivatingComponent();
            this.unity_uitext.color = color;
        }

        public void SetTextWithColor( string text, string colorstr)
        {
            if (string.IsNullOrEmpty(colorstr))
                this.SetText(text);
            else
                this.SetText($"<color={colorstr}>{text}</color>");
        }
    }
}
