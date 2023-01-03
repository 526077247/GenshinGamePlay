using UnityEngine.Events;
using UnityEngine.UI;

namespace TaoTie
{
    public class UIDropdown:UIBaseContainer,IOnDestroy
    {
        public Dropdown dropdown;
        public UnityAction<int> onValueChanged;

        #region override

        public void OnDestroy()
        {
            RemoveOnValueChanged();
        }

        #endregion
        
        void ActivatingComponent()
        {
            if (this.dropdown == null)
            {
                this.dropdown = this.GetGameObject().GetComponent<Dropdown>();
                if (this.dropdown == null)
                {
                    Log.Error($"添加UI侧组件UIDropdown时，物体{this.GetGameObject().name}上没有找到Dropdown组件");
                }
            }
        }
        public void SetOnValueChanged( UnityAction<int> callback)
        {
            this.ActivatingComponent();
            this.RemoveOnValueChanged();
            this.onValueChanged = callback;
            this.dropdown.onValueChanged.AddListener(this.onValueChanged);
        }
        public void RemoveOnValueChanged()
        {
            if (this.onValueChanged != null)
            {
                this.dropdown.onValueChanged.RemoveListener(this.onValueChanged);
                this.onValueChanged = null;
            }
        }
        public int GetValue()
        {
            this.ActivatingComponent();
            return this.dropdown.value;
        }
        public void SetValue( int value)
        {
            this.ActivatingComponent();
            this.dropdown.value = value;
        }
    }
}