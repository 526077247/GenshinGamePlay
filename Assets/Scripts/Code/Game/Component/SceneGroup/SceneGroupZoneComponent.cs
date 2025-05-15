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

        private void OnTriggerEnterEvt(long other)
        {
            if (!innerEntity.Contains(other))
            {
                innerEntity.Add(other);
                Messager.Instance.Broadcast(sceneGroupId, MessageId.SceneGroupEvent, new EnterZoneEvent()
                {
                    ZoneLocalId = localId,
                    ZoneEntityId = Id,
                    EntityId = other
                });
            }
            else
            {
                Log.Error($"重复进入sceneGroupId{sceneGroupId} localId{localId} entityId{other}");
            }
        }

        private void OnTriggerExitEvt(long id)
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