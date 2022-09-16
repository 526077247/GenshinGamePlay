namespace TaoTie
{
    public class UIBaseView:UIBaseContainer
    {
        
        /// <summary>
        /// 关闭自身
        /// </summary>
        public async ETTask CloseSelf()
        {
            await UIManager.Instance.CloseWindow(this);
        }
    }
}