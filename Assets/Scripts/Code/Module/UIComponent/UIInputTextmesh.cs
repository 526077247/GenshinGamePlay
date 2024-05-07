using System;
using UnityEngine.Events;

namespace TaoTie
{
    public class UIInputTextmesh:UIBaseContainer,IOnDestroy
    {
        private TMPro.TMP_InputField input;
        
        private UnityAction<string> onValueChange;

        private UnityAction<string> onEndEdit;
        public void OnDestroy()
        {
            this.RemoveOnValueChanged();
            this.RemoveOnEndEdit();
        }
        void ActivatingComponent()
        {
            if (this.input == null)
            {
                this.input = this.GetGameObject().GetComponent<TMPro.TMP_InputField>();
                if (this.input == null)
                {
                    Log.Error($"添加UI侧组件UIInputTextmesh时，物体{this.GetGameObject().name}上没有找到TMPro.TMP_InputField组件");
                }
            }
        }
        public string GetText()
        {
            this.ActivatingComponent();
            return this.input.text;
        }

        public void SetText(string text)
        {
            this.ActivatingComponent();
            this.input.text = text;
        }

        public void SetOnValueChanged(Action func)
        {
            this.ActivatingComponent();
            this.RemoveOnValueChanged();
            this.onValueChange = (a) =>
            {
                func?.Invoke();
            };
            this.input.onValueChanged.AddListener(this.onValueChange);
        }

        public void RemoveOnValueChanged()
        {
            if(this.onValueChange!=null)
                this.input.onValueChanged.RemoveListener(this.onValueChange);
            this.onValueChange = null;
        }
        
        
        public void SetOnEndEdit(Action func)
        {
            this.ActivatingComponent();
            this.RemoveOnEndEdit();
            this.onEndEdit = (a) =>
            {
                func?.Invoke();
            };
            this.input.onEndEdit.AddListener(this.onEndEdit);
        }
        
        public void RemoveOnEndEdit()
        {
            if(this.onEndEdit!=null)
                this.input.onEndEdit.RemoveListener(this.onEndEdit);
            this.onEndEdit = null;
        }
    }
}