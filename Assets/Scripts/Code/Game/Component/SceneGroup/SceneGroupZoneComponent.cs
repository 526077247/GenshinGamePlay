using System.Collections.Generic;

namespace TaoTie
{
    public class SceneGroupZoneComponent: Component,IComponent<int,long>
    {
        private int LocalId;

        private long sceneGroupId;
        private SceneGroup sceneGroup => parent.Parent.Get<SceneGroup>(sceneGroupId);
        
        private List<long> innerEntity;
        #region IComponent

        public void Init(int p1, long p2)
        {
            LocalId = p1;
            sceneGroupId = p2;
        }

        public void Destroy()
        {
            sceneGroupId = 0;
            LocalId = 0;
        }

        #endregion
        
        public int GetRegionEntityCount(EntityType type)
        {
            if (innerEntity.Count == 0) return 0;
            var uc = parent.Parent;
            int count = 0;
            for (int i = 0; i < innerEntity.Count; i++)
            {
                var u = uc.Get<Unit>(innerEntity[i]);
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