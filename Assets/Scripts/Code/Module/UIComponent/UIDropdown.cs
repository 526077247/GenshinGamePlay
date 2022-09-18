using UnityEngine.Events;
using UnityEngine.UI;

namespace TaoTie
{
    public class UIDropdown:UIBaseContainer,IOnDestroy
    {
        public Dropdown unity_uidropdown;
        public UnityAction<int> __onValueChanged;

        #region override

        public void OnDestroy()
        {
            RemoveOnValueChanged();
        }

        #endregion
        
        void ActivatingComponent()
        {
            if (this.unity_uidropdown == null)
            {
                this.unity_uidropdown = this.GetGameObject().GetComponent<Dropdown>();
                if (this.unity_uidropdown == null)
                {
                    Log.Error($"添加UI侧组件UIDropdown时，物体{this.GetGameObject().name}上没有找到Dropdown组件");
                }
            }
        }
        public void SetOnValueChanged( UnityAction<int> callback)
        {
            this.ActivatingComponent();
            this.RemoveOnValueChanged();
            this.__onValueChanged = callback;
            this.unity_uidropdown.onValueChanged.AddListener(this.__onValueChanged);
        }
        public void RemoveOnValueChanged()
        {
            if (this.__onValueChanged != null)
            {
                this.unity_uidropdown.onValueChanged.RemoveListener(this.__onValueChanged);
                this.__onValueChanged = null;
            }
        }
        public int GetValue()
        {
            this.ActivatingComponent();
            return this.unity_uidropdown.value;
        }
        public void SetValue( int value)
        {
            this.ActivatingComponent();
            this.unity_uidropdown.value = value;
        }
    }
}