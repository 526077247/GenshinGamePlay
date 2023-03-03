using System.Collections.Generic;

namespace TaoTie
{
    public class SceneGroupZoneComponent: Component,IComponent<int,long>
    {
        private int localId;

        private long sceneGroupId;
        private SceneGroup SceneGroup => Parent.Parent.Get<SceneGroup>(sceneGroupId);
        
        private List<long> InnerEntity;
        #region IComponent

        public void Init(int p1, long p2)
        {
            localId = p1;
            sceneGroupId = p2;
        }

        public void Destroy()
        {
            sceneGroupId = 0;
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
                    Log.Error("SceneGroupZone里有没清除的id");
                }
            }

            return count;
        }
    }
}