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

    public static class PhysicsHelper
    {
        private static readonly Collider[] colliders = new Collider[64];
        private static readonly HitInfo[] hitInfos = new HitInfo[64];
        private static readonly int layer = LayerMask.GetMask("Entity");
        private static readonly Dictionary<long, int> idMapIndex = new Dictionary<long, int> ();

        public static int OverlapBoxNonAlloc(Vector3 center, Vector3 halfExtents, Quaternion orientation,
            EntityType[] filter,
            out HitInfo[] entities)
        {
            entities = hitInfos;
            var len = Physics.OverlapBoxNonAlloc(center, halfExtents, colliders, orientation, layer,
                QueryTriggerInteraction.Ignore);
            return Filter(filter,len,center);
        }
        public static int OverlapSphereNonAlloc(Vector3 center, float radius, EntityType[] filter,
            out HitInfo[] entities)
        {
            entities = hitInfos;
            var len = Physics.OverlapSphereNonAlloc(center, radius, colliders, layer, QueryTriggerInteraction.Ignore);
            return Filter(filter,len,center);
        }

        private static int Filter(EntityType[] filter, int len, Vector3 startPos)
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
                            Log.Error("IntersectRay what's fuck?");
                            continue;
                        }
                        int index = count;
                        if (idMapIndex.TryGetValue(e.Id, out int oldIndex))
                        {
                            if (hitInfos[oldIndex].Distance < dis)
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
    }
}