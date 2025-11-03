using System;
using UnityEngine.Events;

namespace TaoTie
{
    public class UIPointerClick:UIBaseContainer,IOnDestroy
    {
        private Action onClick;
        private PointerClick pointerClick;

        #region override

        public void OnDestroy()
        {
            this.RemoveOnClick();
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
        /// <summary>
        /// 虚拟点击
        /// </summary>
        public void Click()
        {
            OnClickPointer();
        }

        private void OnClickPointer()
        {
            // SoundManager.Instance.PlaySound("Audio/Sound/Common_Click.mp3");
            onClick?.Invoke();
        }
        
        public void SetOnClick(Action callback)
        {
            this.ActivatingComponent();
            this.RemoveOnClick();
            this.onClick = callback;
            this.pointerClick.onClick.AddListener(OnClickPointer);
        }

        public void RemoveOnClick()
        {
            if (this.onClick != null)
                this.pointerClick.onClick.RemoveListener(OnClickPointer);
            this.onClick = null;
        }

        public void SetEnabled(bool flag)
        {
            this.ActivatingComponent();
            this.pointerClick.enabled = flag;
        }
    }
}