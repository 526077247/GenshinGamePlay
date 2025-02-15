namespace TaoTie
{
    public partial class CameraManager:IManager
    {
        #region IManager
        public static CameraManager Instance { get; private set; }
        public void Init()
        {
            Instance = this;
        }

        public void Destroy()
        {
            Instance = null;
        }

        #endregion

        public partial ETTask LoadAsync();
    }
}