using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    public class SceneGroupZoneComponent: Component,IComponent<int,long>
    {
        private int localId;

        private long sceneGroupId;

        private List<long> innerEntity;
        private TriggerComponent colliderComponent;
        #region IComponent

        public void Init(int p1, long p2)
        {
            innerEntity = new List<long>();
            localId = p1;
            sceneGroupId = p2;

            colliderComponent = parent.GetComponent<TriggerComponent>();
            colliderComponent.OnTriggerEnterEvt += OnTriggerEnterEvt;
            colliderComponent.OnTriggerExitEvt += OnTriggerExitEvt;
        }

        public void Destroy()
        {
            if (colliderComponent != null)
            {
                colliderComponent.OnTriggerExitEvt -= OnTriggerEnterEvt;
                colliderComponent.OnTriggerEnterEvt -= OnTriggerExitEvt;
                colliderComponent = null;
            }
            sceneGroupId = 0;
            localId = 0;
            innerEntity = null;
        }

        #endregion

        private void OnTriggerEnterEvt(Entity other)
        {
            if (!innerEntity.Contains(other.Id))
            {
                innerEntity.Add(other.Id);
                Messager.Instance.Broadcast(sceneGroupId, MessageId.SceneGroupEvent, new EnterZoneEvent()
                {
                    ZoneLocalId = localId,
                    ZoneEntityId = Id,
                    EntityId = other.Id
                });
            }
            else
            {
                Log.Error($"重复进入sceneGroupId{sceneGroupId} localId{localId} entityId{other.Id}");
            }
        }

        private void OnTriggerExitEvt(Entity other)
        {
            if (innerEntity.Contains(other.Id))
            {
                Messager.Instance.Broadcast(sceneGroupId,MessageId.SceneGroupEvent,new ExitZoneEvent()
                {
                    ZoneLocalId = localId,
                    ZoneEntityId = Id,
                    EntityId = other.Id
                });
                innerEntity.Remove(other.Id);
            }
            else
            {
                Log.Error($"重复离开 sceneGroupId{sceneGroupId} localId{localId} entityId{other.Id}");
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