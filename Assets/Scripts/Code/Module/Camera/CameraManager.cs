namespace TaoTie
{
    public partial class CameraManager:IManager
    {
        #region IManager
        public static CameraManager Instance { get; private set; }
        public void Init()
        {
            Instance = this;
            AfterInit();
        }

        public void Destroy()
        {
            Instance = null;
        }

        #endregion

        private partial void AfterInit();
    }
}