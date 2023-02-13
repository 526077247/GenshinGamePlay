namespace TaoTie
{
    public class GearActorComponent: Component,IComponent<int,long>
    {
        public int localId { get; private set; }

        private long gearId;
        public Gear gear => Parent.Parent.Get<Gear>(gearId);
        #region IComponent

        public void Init(int p1, long p2)
        {
            localId = p1;
            gearId = p2;
        }

        public void Destroy()
        {
            gear?.OnActorRelease(localId);
            gearId = 0;
            localId = 0;
        }

        #endregion
        
        /// <summary>
        /// 与Gear脱钩，之后gear切换group，也不会销毁该actor
        /// </summary>
        public void RemoveFromGear()
        {
            gear?.OnActorRelease(localId);
        }
    }
}