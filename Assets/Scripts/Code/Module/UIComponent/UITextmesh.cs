using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace TaoTie
{
    public class UITextmesh : UIBaseContainer,II18N
    {
        private TMPro.TMP_Text text;
        private I18NText i18nCompTouched;
        private I18NKey textKey;
        private object[] keyParams;
        
        void ActivatingComponent()
        {
            if (this.text == null)
            {
                this.text = this.GetGameObject().GetComponent<TMPro.TMP_Text>();
                if (this.text == null)
                {
                    this.text = this.GetGameObject().AddComponent<TMPro.TMP_Text>();
                    Log.Info($"添加UI侧组件UIText时，物体{this.GetGameObject().name}上没有找到Text组件");
                }
                this.i18nCompTouched = this.GetGameObject().GetComponent<I18NText>();
            }
        }

        //当手动修改text的时候，需要将mono的i18textcomponent给禁用掉
        void DisableI18Component(bool enable = false)
        {
            this.ActivatingComponent();
            if (this.i18nCompTouched != null)
            {
                this.i18nCompTouched.enabled = enable;
                if (!enable)
                    Log.Warning($"组件{this.GetGameObject().name}, text在逻辑层进行了修改，所以应该去掉去预设里面的I18N组件，否则会被覆盖");
            }
        }

        public string GetText()
        {
            this.ActivatingComponent();
            return this.text.text;
        }

        public void SetText(string text)
        {
            this.DisableI18Component();
            this.textKey = default;
            this.text.text = text;
        }
        public void SetI18NKey(I18NKey key)
        {
            if (key == default)
            {
                this.SetText("");
                return;
            }
            this.DisableI18Component();
            this.textKey = key;
            this.SetI18NText(null);
        }
        public void SetI18NKey(I18NKey key, params object[] paras)
        {
            if (key == default)
            {
                this.SetText("");
                return;
            }

            this.DisableI18Component();
            this.textKey = key;
            this.SetI18NText(paras);
        }

        public void SetI18NText(params object[] paras)
        {
            if (textKey == default)
            {
                Log.Error("there is not key ");
            }
            else
            {
                this.DisableI18Component();
                this.keyParams = paras;
                if (I18NManager.Instance.I18NTryGetText(this.textKey, out var text) && paras != null)
                    text = string.Format(text, paras);
                this.text.text = text;
            }
        }

        public void OnLanguageChange()
        {
            this.ActivatingComponent();
            {
                if (textKey != default)
                {
                    if (I18NManager.Instance.I18NTryGetText(this.textKey, out var text) && this.keyParams != null)
                        text = string.Format(text, this.keyParams);
                    this.text.text = text;
                }
            }
        }

        public void SetTextColor(Color color)
        {
            this.ActivatingComponent();
            this.text.color = color;
        }

        public void SetTextWithColor(string text, string colorstr)
        {
            if (string.IsNullOrEmpty(colorstr))
                this.SetText(text);
            else
                this.SetText($"<color={colorstr}>{text}</color>");
        }
        
                
        public int GetCharacterCount()
        {
            ActivatingComponent();
            return text.CharacterCount;
        }

        public void SetMaxVisibleCharacters(int count)
        {
            ActivatingComponent();
            text.maxVisibleCharacters = count;
        }
        /// <summary>
        /// 获取最后一个字符右下角坐标
        /// </summary>
        /// <returns></returns>
        public Vector3 GetLastCharacterLocalPosition()
        {
            ActivatingComponent();
            if (text.m_textInfo.characterInfo != null && text.m_textInfo.characterInfo.Length > 0)
            {
                var info = text.m_textInfo.characterInfo[text.m_textInfo.characterCount - 1];
                return info.vertex_BR.position;
            }

            var rect = text.rectTransform.rect;
            return new Vector3(-rect.width / 2, -rect.height / 2, 0);
        }
        /// <summary>
        /// 获取指定字符右下角坐标
        /// </summary>
        /// <returns></returns>
        public Vector3 GetCharacterLocalPosition(int index)
        {
            ActivatingComponent();
            if (text.m_textInfo.characterInfo != null && text.m_textInfo.characterInfo.Length > index)
            {
                var info = text.m_textInfo.characterInfo[index];
                return info.vertex_BR.position;
            }

            var rect = text.rectTransform.rect;
            return new Vector3(-rect.width / 2, -rect.height / 2, 0);
        }
    }
}
