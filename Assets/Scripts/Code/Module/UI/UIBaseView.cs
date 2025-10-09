namespace TaoTie
{
    public class UIBaseView:UIBaseContainer
    {
        public virtual bool CanBack => false;
        /// <summary>
        /// 关闭自身
        /// </summary>
        public virtual async ETTask CloseSelf()
        {
            var close = await UIManager.Instance.CloseBox(this);
            if(!close) await UIManager.Instance.CloseWindow(this);
        }

        public virtual ETTask OnInputKeyBack()
        {
            return CloseSelf();
        }
    }
}