using System;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TaoTie
{
    public class UIToggle: UIBaseContainer,IOnDestroy
    {
        public Toggle unity_uitoggle;
        public UnityAction<bool> CallBack;

        #region override

        public void OnDestroy()
        {
            RemoveOnValueChanged();
        }

        #endregion
        
        void ActivatingComponent()
        {
            if (this.unity_uitoggle == null)
            {
                this.unity_uitoggle = this.GetGameObject().GetComponent<Toggle>();
                if (this.unity_uitoggle == null)
                {
                    Log.Error($"添加UI侧组件UIToggle时，物体{this.GetGameObject().name}上没有找到Toggle组件");
                }
            }
        }
        public bool GetIsOn()
        {
            this.ActivatingComponent();
            return this.unity_uitoggle.isOn;
        }

        public void SetIsOn(bool ison,bool broadcast = true)
        {
            this.ActivatingComponent();
            if(broadcast)
                this.unity_uitoggle.isOn = ison;
            else
                this.unity_uitoggle.SetIsOnWithoutNotify(ison);
        }
        
        public void SetOnValueChanged(Action<bool> cb)
        {
            this.ActivatingComponent();
            this.CallBack = (a)=>
            {
                cb?.Invoke(a);
            };
            this.unity_uitoggle.onValueChanged.AddListener(this.CallBack);
        }
        
        public void RemoveOnValueChanged()
        {
            if (this.CallBack != null)
            {
                this.unity_uitoggle.onValueChanged.RemoveListener(this.CallBack);
            }
            
        }
    }
}