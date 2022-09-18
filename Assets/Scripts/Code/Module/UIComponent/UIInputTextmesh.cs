using System;
using UnityEngine.Events;

namespace TaoTie
{
    public class UIInputTextmesh:UIBaseContainer
    {
        public TMPro.TMP_InputField unity_uiinput;
        
        public UnityAction<string> __OnValueChange;

        public UnityAction<string> __OnEndEdit;
        
        void ActivatingComponent()
        {
            if (this.unity_uiinput == null)
            {
                this.unity_uiinput = this.GetGameObject().GetComponent<TMPro.TMP_InputField>();
                if (this.unity_uiinput == null)
                {
                    Log.Error($"添加UI侧组件UIInputTextmesh时，物体{this.GetGameObject().name}上没有找到TMPro.TMP_InputField组件");
                }
            }
        }
        public string GetText()
        {
            this.ActivatingComponent();
            return this.unity_uiinput.text;
        }

        public void SetText(string text)
        {
            this.ActivatingComponent();
            this.unity_uiinput.text = text;
        }

        public void SetOnValueChanged(Action func)
        {
            this.ActivatingComponent();
            this.RemoveOnValueChanged();
            this.__OnValueChange = (a) =>
            {
                func?.Invoke();
            };
            this.unity_uiinput.onValueChanged.AddListener(this.__OnValueChange);
        }

        public void RemoveOnValueChanged()
        {
            if(this.__OnValueChange!=null)
                this.unity_uiinput.onValueChanged.RemoveListener(this.__OnValueChange);
        }
        
        
        public void SetOnEndEdit(Action func)
        {
            this.ActivatingComponent();
            this.RemoveOnEndEdit();
            this.__OnEndEdit = (a) =>
            {
                func?.Invoke();
            };
            this.unity_uiinput.onEndEdit.AddListener(this.__OnEndEdit);
        }
        
        public void RemoveOnEndEdit()
        {
            if(this.__OnEndEdit!=null)
                this.unity_uiinput.onEndEdit.RemoveListener(this.__OnEndEdit);
        }
    }
}