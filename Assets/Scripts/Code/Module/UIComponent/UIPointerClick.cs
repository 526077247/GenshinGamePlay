using UnityEngine.Events;

namespace TaoTie
{
    public class UIPointerClick:UIBaseContainer,IOnDestroy
    {
        public UnityAction __onclick;
        public PointerClick unity_pointerclick;

        #region override

        public void OnDestroy()
        {
            if (__onclick != null)
                unity_pointerclick.onClick.RemoveListener(__onclick);
            __onclick = null;
        }

        #endregion
        
        void ActivatingComponent()
        {
            if (this.unity_pointerclick == null)
            {
                this.unity_pointerclick = this.GetGameObject().GetComponent<PointerClick>();
                if (this.unity_pointerclick == null)
                {
                    this.unity_pointerclick = this.GetGameObject().AddComponent<PointerClick>();
                    Log.Info($"添加UI侧组件UIPointerClick时，物体{this.GetGameObject().name}上没有找到PointerClick组件");
                }
            }
        }
        //虚拟点击
        public void Click()
        {
            this.__onclick?.Invoke();
        }

        public void SetOnClick(UnityAction callback)
        {
            this.ActivatingComponent();
            this.RemoveOnClick();
            this.__onclick = () =>
            {
                //AkSoundEngine.PostEvent("ConFirmation", Camera.main.gameObject);
                callback();
            };
            this.unity_pointerclick.onClick.AddListener(this.__onclick);
        }

        public void RemoveOnClick()
        {
            if (this.__onclick != null)
                this.unity_pointerclick.onClick.RemoveListener(this.__onclick);
            this.__onclick = null;
        }

        public void SetEnabled(bool flag)
        {
            this.ActivatingComponent();
            this.unity_pointerclick.enabled = flag;
        }
    }
}