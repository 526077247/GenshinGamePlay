using System.Collections.Generic;

namespace TaoTie
{
    public class GearZoneComponent: Component,IComponent<int,long>
    {
        private int localId;

        private long gearId;
        private Gear gear => Parent.Parent.Get<Gear>(gearId);
        
        private List<long> InnerEntity;
        #region IComponent

        public void Init(int p1, long p2)
        {
            localId = p1;
            gearId = p2;
        }

        public void Destroy()
        {
            gearId = 0;
            localId = 0;
        }

        #endregion
        
        public int GetRegionEntityCount(EntityType type)
        {
            if (InnerEntity.Count == 0) return 0;
            var uc = Parent.Parent;
            int count = 0;
            for (int i = 0; i < InnerEntity.Count; i++)
            {
                var u = uc.Get<Unit>(InnerEntity[i]);
                if (u != null)
                {
                    if (u.Type == type)
                    {
                        count++;
                    }
                }
                else
                {
                    Log.Error("GearZone里有没清除的id");
                }
            }

            return count;
        }
    }
}