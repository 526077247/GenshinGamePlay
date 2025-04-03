using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    public class SceneGroupZoneComponent: Component,IComponent<int,long,GameObject>
    {
        private int localId;

        private long sceneGroupId;
        private SceneGroup sceneGroup => parent.Parent.Get<SceneGroup>(sceneGroupId);
        
        private List<long> innerEntity;
        private ColliderComponent colliderComponent;
        private GameObject zone;
        #region IComponent

        public void Init(int p1, long p2,GameObject obj)
        {
            innerEntity = new List<long>();
            localId = p1;
            sceneGroupId = p2;
            zone = obj;
            colliderComponent = zone.GetComponent<ColliderComponent>();
            if (colliderComponent == null)
            {
                colliderComponent = zone.AddComponent<ColliderComponent>();
            }
            colliderComponent.OnEntityTrigger = OnEntityTrigger;
        }

        public void Destroy()
        {
            if (colliderComponent != null)
            {
                colliderComponent.OnEntityTrigger = null;
                colliderComponent = null;
            }
            sceneGroupId = 0;
            localId = 0;
            innerEntity = null;
            if (zone != null)
            {
                GameObject.Destroy(zone);
                zone = null;
            }
        }

        #endregion

        private void OnEntityTrigger(long id, bool isEnter)
        {
            if (isEnter)
            {
                if (!innerEntity.Contains(id))
                {
                    innerEntity.Add(id);
                    Messager.Instance.Broadcast(sceneGroupId,MessageId.SceneGroupEvent,new EnterZoneEvent()
                    {
                        ZoneLocalId = localId,
                        ZoneEntityId = Id,
                        EntityId = id
                    });
                }
                else
                {
                    Log.Error($"重复进入sceneGroupId{sceneGroupId} localId{localId} entityId{id}");
                }
            }
            else
            {
                if (innerEntity.Contains(id))
                {
                    Messager.Instance.Broadcast(sceneGroupId,MessageId.SceneGroupEvent,new ExitZoneEvent()
                    {
                       ZoneLocalId = localId,
                       ZoneEntityId = Id,
                       EntityId = id
                    });
                    innerEntity.Remove(id);
                }
                else
                {
                    Log.Error($"重复离开 sceneGroupId{sceneGroupId} localId{localId} entityId{id}");
                }
            }
        }
        
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