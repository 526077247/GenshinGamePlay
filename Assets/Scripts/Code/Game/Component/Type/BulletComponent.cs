namespace TaoTie
{
    public class BulletComponent: Component,IComponent,IComponent<long>
    {
        public long CreateTime;
        #region IComponent

        public void Init()
        {
            CreateTime = GameTimerManager.Instance.GetTimeNow();
        }
        public void Init(long time)
        {
            CreateTime = time;
        }
        public void Destroy()
        {
            CreateTime = default;
        }

        #endregion
    }
}