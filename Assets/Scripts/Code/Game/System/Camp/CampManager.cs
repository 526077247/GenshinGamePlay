namespace TaoTie
{
    public class CampManager: IManager
    {
        public static CampManager Instance { get; private set; }
        #region IManager

        public void Init()
        {
            Instance = this;
        }

        public void Destroy()
        {
            Instance = null;
        }

        #endregion
    }
}