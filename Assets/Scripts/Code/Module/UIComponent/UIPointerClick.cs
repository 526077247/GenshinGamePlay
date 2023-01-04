using UnityEngine.Events;

namespace TaoTie
{
    public class UIPointerClick:UIBaseContainer,IOnDestroy
    {
        private UnityAction onClick;
        private PointerClick pointerClick;

        #region override

        public void OnDestroy()
        {
            if (onClick != null)
                pointerClick.onClick.RemoveListener(onClick);
            onClick = null;
        }

        #endregion
        
        void ActivatingComponent()
        {
            if (this.pointerClick == null)
            {
                this.pointerClick = this.GetGameObject().GetComponent<PointerClick>();
                if (this.pointerClick == null)
                {
                    this.pointerClick = this.GetGameObject().AddComponent<PointerClick>();
                    Log.Info($"添加UI侧组件UIPointerClick时，物体{this.GetGameObject().name}上没有找到PointerClick组件");
                }
            }
        }
        //虚拟点击
        public void Click()
        {
            this.onClick?.Invoke();
        }

        public void SetOnClick(UnityAction callback)
        {
            this.ActivatingComponent();
            this.RemoveOnClick();
            this.onClick = () =>
            {
                //AkSoundEngine.PostEvent("ConFirmation", Camera.main.gameObject);
                callback?.Invoke();
            };
            this.pointerClick.onClick.AddListener(this.onClick);
        }

        public void RemoveOnClick()
        {
            if (this.onClick != null)
                this.pointerClick.onClick.RemoveListener(this.onClick);
            this.onClick = null;
        }

        public void SetEnabled(bool flag)
        {
            this.ActivatingComponent();
            this.pointerClick.enabled = flag;
        }
    }
}