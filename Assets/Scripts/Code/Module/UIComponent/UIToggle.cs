using System;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TaoTie
{
    public class UIToggle: UIBaseContainer,IOnDestroy
    {
        private Toggle toggle;
        private UnityAction<bool> callBack;

        #region override

        public void OnDestroy()
        {
            RemoveOnValueChanged();
        }

        #endregion
        
        void ActivatingComponent()
        {
            if (this.toggle == null)
            {
                this.toggle = this.GetGameObject().GetComponent<Toggle>();
                if (this.toggle == null)
                {
                    Log.Error($"添加UI侧组件UIToggle时，物体{this.GetGameObject().name}上没有找到Toggle组件");
                }
            }
        }
        public bool GetIsOn()
        {
            this.ActivatingComponent();
            return this.toggle.isOn;
        }

        public void SetIsOn(bool ison,bool broadcast = true)
        {
            this.ActivatingComponent();
            if(broadcast)
                this.toggle.isOn = ison;
            else
                this.toggle.SetIsOnWithoutNotify(ison);
        }
        
        public void SetOnValueChanged(Action<bool> cb)
        {
            this.ActivatingComponent();
            this.callBack = (a)=>
            {
                cb?.Invoke(a);
            };
            this.toggle.onValueChanged.AddListener(this.callBack);
        }
        
        public void RemoveOnValueChanged()
        {
            if (this.callBack != null)
            {
                this.toggle.onValueChanged.RemoveListener(this.callBack);
            }
            
        }
    }
}