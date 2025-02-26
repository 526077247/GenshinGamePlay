namespace TaoTie
{
    public class UIBaseView:UIBaseContainer
    {
        public virtual bool CanBack => false;
        /// <summary>
        /// 关闭自身
        /// </summary>
        public async ETTask CloseSelf()
        {
            await UIManager.Instance.CloseWindow(this);
        }

        public virtual ETTask OnInputKeyBack()
        {
            return CloseSelf();
        }
    }
}