using System;
using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    public static class PhysicsHelper
    {
        private static RaycastHit[] raycastHits = new RaycastHit[8];
        private static readonly Collider[] colliders = new Collider[64];
        private static readonly HitInfo[] hitInfos = new HitInfo[64];
        private static readonly long[] entities = new long[64];
        private static readonly int entity = LayerMask.GetMask("Entity");
        private static readonly int hitbox = LayerMask.GetMask("HitBox");
        private static readonly int hitscene = LayerMask.GetMask("HitScene");
        private static readonly int defaultL = LayerMask.GetMask("Default");
        private static readonly Dictionary<long, int> idMapIndex = new Dictionary<long, int> ();

        #region Raycast

        public static bool RaycastNonAlloc(
            Vector3 origin,
            Vector3 direction,
            out RaycastHit result,
            float maxDistance,
            int layerMask,
            QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            var len = Physics.RaycastNonAlloc(origin, direction, raycastHits, maxDistance, layerMask,
                queryTriggerInteraction);
            result = raycastHits[0];
            for (int i = 1; i < len; i++)
            {
                if (raycastHits[i].distance < result.distance)
                {
                    result = raycastHits[i];
                }
            }
            return len > 0;
        }

        public static bool SphereCastNonAlloc(
            Vector3 origin,
            float radius,
            Vector3 direction,
            out RaycastHit result,
            float maxDistance,
            int layerMask,
            QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            var len = Physics.SphereCastNonAlloc(origin, radius, direction, raycastHits, maxDistance, layerMask,
                queryTriggerInteraction);
            result = raycastHits[0];
            for (int i = 1; i < len; i++)
            {
                if (raycastHits[i].distance < result.distance)
                {
                    result = raycastHits[i];
                }
            }
            return len > 0;
        }
        #endregion
        #region Entity
        public static int OverlapCapsuleNonAlloc(Vector3 p1, Vector3 p2, float radius, 
            EntityType[] filter, out long[] res)
        {
            res = entities;
            if (filter == null) return 0;

            var len = Physics.OverlapCapsuleNonAlloc(p1, p2,radius, colliders, entity,
                QueryTriggerInteraction.Ignore);
            return FilterEntity(filter,len);
        }
        public static int OverlapBoxNonAllocEntity(Vector3 center, Vector3 halfExtents, Quaternion orientation,
            EntityType[] filter, out long[] res)
        {
            res = entities;
            if (filter == null) return 0;
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
                    if (e.EntityType == filter[j])
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
            EntityType[] filter, CheckHitLayerType type, out HitInfo[] res)
        {
            res = hitInfos;
            if (filter == null) return 0;
            var len = Physics.OverlapBoxNonAlloc(center, halfExtents, colliders, orientation, GetHitLayer(type),
                QueryTriggerInteraction.Collide);
            return FilterHitInfo(filter,len,center);
        }
        public static int OverlapSphereNonAllocHitInfo(Vector3 center, float radius, EntityType[] filter,
            CheckHitLayerType type, out HitInfo[] res)
        {
            res = hitInfos;
            var len = Physics.OverlapSphereNonAlloc(center, radius, colliders, GetHitLayer(type), QueryTriggerInteraction.Collide);
            return FilterHitInfo(filter,len,center);
        }
        public static int OverlapCapsuleNonAllocHitInfo(Vector3 p1, Vector3 p2, float radius, 
            EntityType[] filter,  CheckHitLayerType type, out HitInfo[] res)
        {
            res = hitInfos;
            if (filter == null) return 0;
            var len = Physics.OverlapCapsuleNonAlloc(p1, p2,radius, colliders, GetHitLayer(type),
                QueryTriggerInteraction.Collide);
            return FilterHitInfo(filter, len, (p1 + p2) / 2);
        }
        public static int OverlapColliderNonAllocHitInfo(ColliderBoxComponent colliderBox, EntityType[] filter,
            CheckHitLayerType type, out HitInfo[] res)
        {
            res = hitInfos;
            if (colliderBox == null) return 0;
            int len = 0;
            var hitLayer = GetHitLayer(type);
            for (int i = 0; i < colliderBox.TriggerList.Count; i++)
            {
                var other = colliderBox.TriggerList[i];
                if ((2<<other.gameObject.layer & hitLayer) != 0)
                {
                    colliders[len] = other;
                    len++;
                    if (len == colliders.Length)
                    {
                        break;
                    }
                }
            }
            Vector3 center = colliderBox.GetComponent<Collider>().bounds.center;
            return FilterHitInfo(filter,len,center);
        }

        public static int OverlapColliderNonAllocHitInfo(ColliderBoxComponent colliderBox, Collider[] triggers,
            EntityType[] filter, CheckHitLayerType type, out HitInfo[] res)
        {
            res = hitInfos;
            if (colliderBox == null || triggers == null || triggers.Length == 0) return 0;
            int len = 0;
            var hitLayer = GetHitLayer(type);
            for (int i = 0; i < triggers.Length; i++)
            {
                var other = triggers[i];
                if ((2<<other.gameObject.layer & hitLayer) != 0)
                {
                    colliders[len] = other;
                    len++;
                    if (len == colliders.Length)
                    {
                        break;
                    }
                }
            }

            Vector3 center = colliderBox.GetComponent<Collider>().bounds.center;
            return FilterHitInfo(filter, len, center);
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
            bool isAll = false;
            for (int i = 0; i < len; i++)
            {
                Collider collider = colliders[i];
                if (collider == null || collider.transform == null) continue;
                var gitBox = collider.GetComponent<HitBoxComponent>();
                if (gitBox == null) continue;
                var e = collider.transform.GetComponentInParent<EntityComponent>();
                if (e == null) continue;
                for (int j = 0; j < filter.Length; j++)
                {
                    if (filter[j] == EntityType.ALL) isAll = true;
                    if (isAll || e.EntityType == filter[j])
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
                            HitPos = ray.GetPoint(dis),
                            HitBoxType = gitBox.HitBoxType
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

        #region Seen

        public static bool LinecastScene(Vector3 start, Vector3 end, out Vector3 pos)
        {
            var res = Physics.Linecast(start, end, out var hit, defaultL + hitscene, QueryTriggerInteraction.Ignore);
            if (res)
            {
                pos = hit.point;
            }
            else
            {
                pos = Vector3.zero;
            }

            return res;
        }

        #endregion


        #region Camera

        public static bool SphereCast(Vector3 start, Vector3 end,float radius, LayerMask layer, out RaycastHit hit)
        {
            var dir = end - start;
            return Physics.SphereCast(start,radius,dir, out hit,dir.magnitude, layer);
        }


        #endregion
        
        /// <summary>
        /// 坐标系转换
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="rot"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static Vector3 Transformation(Vector3 pos, Quaternion rot, Vector3 target)
        {
            var posTrans = target - pos;
            return Quaternion.Inverse(rot) * posTrans;
        }
    }
}