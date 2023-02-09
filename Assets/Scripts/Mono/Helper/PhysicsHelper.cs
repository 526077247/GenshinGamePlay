using System;
using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    public struct HitInfo
    {
        public long EntityId;
        public Vector3 HitPos;
        public Vector3 HitDir;
        public float Distance;
    }
    public enum CheckHitLayerType
    {
        OnlyHitBox = 0,
        OnlyHitScene = 1,
        Both = 2
    }
    public static class PhysicsHelper
    {
        private static readonly Collider[] colliders = new Collider[64];
        private static readonly HitInfo[] hitInfos = new HitInfo[64];
        private static readonly long[] entities = new long[64];
        private static readonly int entity = LayerMask.GetMask("Entity");
        private static readonly int hitbox = LayerMask.GetMask("HitBox");
        private static readonly int hitscene = LayerMask.GetMask("HitScene");
        private static readonly Dictionary<long, int> idMapIndex = new Dictionary<long, int> ();

        #region Entity

        public static int OverlapBoxNonAllocEntity(Vector3 center, Vector3 halfExtents, Quaternion orientation,
            EntityType[] filter,
            out long[] res)
        {
            res = entities;
            var len = Physics.OverlapBoxNonAlloc(center, halfExtents, colliders, orientation, entity,
                QueryTriggerInteraction.Ignore);
            return FilterEntity(filter,len);
        }
        public static int OverlapSphereNonAllocEntity(Vector3 center, float radius, EntityType[] filter,
            out long[] res)
        {
            res = entities;
            var len = Physics.OverlapSphereNonAlloc(center, radius, colliders, entity, QueryTriggerInteraction.Ignore);
            return FilterEntity(filter,len);
        }
        private static int FilterEntity(EntityType[] filter, int len)
        {
            int count = 0;
            for (int i = 0; i < len; i++)
            {
                Collider collider = colliders[i];
                if (collider == null || collider.transform == null) continue;
                var e = collider.transform.GetComponentInParent<EntityComponent>();
                if (e == null) continue;
                for (int j = 0; j < filter.Length; j++)
                {
                    if (e.EntityType == filter[i])
                    {
                        int index = count;
                        if (idMapIndex.ContainsKey(e.Id))
                        {
                            continue;
                        }
                        idMapIndex[e.Id] = index;
                        entities[index] = e.Id;
                        count++;
                        break;
                    }
                }
            }
            Array.Clear(colliders, 0, len);
            idMapIndex.Clear();
            return count;
        }
        
        #endregion

        #region HitInfo

        public static int OverlapBoxNonAllocHitInfo(Vector3 center, Vector3 halfExtents, Quaternion orientation,
            EntityType[] filter,CheckHitLayerType type,
            out long[] res)
        {
            res = entities;
            var len = Physics.OverlapBoxNonAlloc(center, halfExtents, colliders, orientation, GetHitLayer(type),
                QueryTriggerInteraction.Ignore);
            return FilterHitInfo(filter,len,center);
        }
        public static int OverlapSphereNonAllocHitInfo(Vector3 center, float radius, EntityType[] filter,
            CheckHitLayerType type,
            out long[] res)
        {
            res = entities;
            var len = Physics.OverlapSphereNonAlloc(center, radius, colliders, GetHitLayer(type), QueryTriggerInteraction.Ignore);
            return FilterHitInfo(filter,len,center);
        }

        private static int GetHitLayer(CheckHitLayerType type)
        {
            switch (type)
            {
                case CheckHitLayerType.Both:
                    return hitbox | hitscene;
                case CheckHitLayerType.OnlyHitBox:
                    return hitbox;
                case CheckHitLayerType.OnlyHitScene:
                    return hitscene;
            }

            return 0;
        }
        private static int FilterHitInfo(EntityType[] filter, int len, Vector3 startPos)
        {
            int count = 0;
            for (int i = 0; i < len; i++)
            {
                Collider collider = colliders[i];
                if (collider == null || collider.transform == null) continue;
                var e = collider.transform.GetComponentInParent<EntityComponent>();
                if (e == null) continue;
                for (int j = 0; j < filter.Length; j++)
                {
                    if (e.EntityType == filter[i])
                    {
                        var center = collider.bounds.center;
                        // 取HitBox中心的连线与HitBox的交点作为受击点
                        Ray ray = new Ray(startPos, (center - startPos).normalized);
                        if (!collider.bounds.IntersectRay(ray, out float dis))
                        {
                            dis = (center - startPos).magnitude;
                        }
                        int index = count;
                        if (idMapIndex.TryGetValue(e.Id, out int oldIndex))
                        {
                            if (hitInfos[oldIndex].Distance > dis)
                            {
                                count--;
                                index = oldIndex;
                            }
                            else
                            {
                                continue;
                            }
                        }
                        idMapIndex[e.Id] = index;
                        hitInfos[index] = new HitInfo()
                        {
                            EntityId = e.Id,
                            Distance = dis,
                            HitDir = ray.direction,
                            HitPos = ray.GetPoint(dis)
                        };
                        count++;
                        break;
                    }
                }
            }
            Array.Clear(colliders, 0, len);
            idMapIndex.Clear();
            return count;
        }
        
        #endregion
    }
}