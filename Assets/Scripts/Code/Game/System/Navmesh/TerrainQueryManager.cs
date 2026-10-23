using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TaoTie
{
    /// <summary>
    /// 地形采样与出生查询注册表。普通地图（NavMesh）等实现负责 <see cref="Register"/>，
    /// 出生、移动等玩法只消费统一结果。
    /// 多个查询按注册顺序尝试，第一个能采样的生效。
    /// 候选点生成（区域内随机散开、圆环搜索）与出生过滤也集中在这里，玩法侧只提供过滤条件。
    /// </summary>
    public class TerrainQueryManager : IManager
    {
        public static TerrainQueryManager Instance;

        /// <summary>圆环搜索的半径步长。</summary>
        public const float DefaultSearchStep = 1f;

        /// <summary>每圈圆环的采样方向数。</summary>
        public const int DefaultSearchDirections = 16;

        /// <summary>区域内随机取点的最大尝试次数。</summary>
        public const int DefaultRandomAttempts = 12;

        /// <summary>区域半径未知时圆环搜索的默认最大半径。</summary>
        public const float DefaultFallbackSearchRadius = 20f;

        /// <summary>没有地形数据时纯物理搜索的最大半径。</summary>
        public const float DefaultPhysicsSearchRadius = 5f;

        private readonly List<ITerrainQuery> queries = new List<ITerrainQuery>();
        private readonly List<IWaterQuery> waterQueries = new List<IWaterQuery>();
        private readonly List<ISpawnFilter> filters = new List<ISpawnFilter>();

        public void Init()
        {
            Instance = this;
            RegisterFilter(new SpawnObstacleFilter());
        }

        public void Destroy()
        {
            Instance = null;
            queries.Clear();
            waterQueries.Clear();
            filters.Clear();
        }

        /// <summary>
        /// 注册地形查询。重复注册同一实例会被忽略。
        /// </summary>
        public void Register(ITerrainQuery query)
        {
            if (query == null || queries.Contains(query)) return;
            queries.Add(query);
            if (query is IWaterQuery water)
            {
                RegisterWater(water);
            }
        }

        /// <summary>
        /// 注销地形查询。
        /// </summary>
        public void Unregister(ITerrainQuery query)
        {
            if (query == null) return;
            queries.Remove(query);
            if (query is IWaterQuery water)
            {
                UnregisterWater(water);
            }
        }

        /// <summary>
        /// 注册水面查询。overrideExisting 为 true 时插入到最前。
        /// </summary>
        public void RegisterWater(IWaterQuery query, bool overrideExisting = false)
        {
            if (query == null || waterQueries.Contains(query)) return;
            if (overrideExisting)
            {
                waterQueries.Insert(0, query);
            }
            else
            {
                waterQueries.Add(query);
            }
        }

        /// <summary>
        /// 注销水面查询。
        /// </summary>
        public void UnregisterWater(IWaterQuery query)
        {
            if (query != null) waterQueries.Remove(query);
        }

        /// <summary>
        /// 注册出生点额外过滤器（物理碰撞、ORCA、禁放区等）。
        /// 所有候选点都必须通过已注册过滤器。
        /// </summary>
        public void RegisterFilter(ISpawnFilter filter)
        {
            if (filter == null || filters.Contains(filter)) return;
            filters.Add(filter);
        }

        /// <summary>
        /// 注销出生点额外过滤器。
        /// </summary>
        public void UnregisterFilter(ISpawnFilter filter)
        {
            if (filter != null) filters.Remove(filter);
        }

        public bool TrySample(Vector3 worldPosition, out TerrainSample sample)
        {
            for (int i = 0; i < queries.Count; i++)
            {
                if (!queries[i].TrySample(worldPosition, out sample)) continue;
                if (TryGetWater(worldPosition, out bool isWater))
                {
                    sample = sample.WithWater(isWater);
                }
                return true;
            }

            sample = default;
            return false;
        }

        /// <summary>
        /// 使用当前优先级最高的水面查询判断单点。无查询负责时返回 false。
        /// </summary>
        public bool TryGetWater(Vector3 worldPosition, out bool isWater)
        {
            for (int i = 0; i < waterQueries.Count; i++)
            {
                if (waterQueries[i].TryGetWater(worldPosition, out isWater)) return true;
            }

            isWater = false;
            return false;
        }

        /// <summary>
        /// 使用当前优先级最高的水面查询判断水平足印。无查询负责时返回 false。
        /// </summary>
        public bool TryOverlapsWater(Vector3 worldPosition, float radius, float baseline, float height,
            out bool overlaps)
        {
            for (int i = 0; i < waterQueries.Count; i++)
            {
                if (waterQueries[i].TryOverlapsWater(worldPosition, radius, baseline, height,
                        out overlaps))
                {
                    return true;
                }
            }

            overlaps = false;
            return false;
        }

        public bool IsWalkable(Vector3 worldPosition, float radius, out TerrainSample center)
        {
            for (int i = 0; i < queries.Count; i++)
            {
                if (!queries[i].TrySample(worldPosition, out center)) continue;
                return queries[i].IsWalkable(worldPosition, radius, out center);
            }

            center = default;
            return false;
        }

        /// <summary>
        /// 求解出生坐标：区域内随机散开 → 原点可用则直接返回 → 否则在周围圆环搜索。
        /// 地形判定之外，还会依次执行已注册的 <see cref="ISpawnFilter"/> 与额外过滤器。
        /// 没有地图注册查询时退化为纯几何圆环搜索（高度不变），同样经过过滤器。
        /// </summary>
        /// <param name="userData">传给 <see cref="ISpawnFilter"/> 的上下文（通常为 Actor）。</param>
        public Vector3 GetSpawnPosition(Vector3 position, SpawnPointOptions options,
            object userData = null)
        {
            if (!TrySample(position, out TerrainSample origin))
            {
                // 无地形数据：与原先一致，先做不受区域限制的随机散开。
                if (options.Range > 0f)
                {
                    position += Quaternion.Euler(0f, Random.Range(0f, 360f), 0f) *
                        Vector3.forward * Random.Range(0f, options.Range);
                }

                return SearchWithoutTerrain(position, options, userData);
            }

            // 随机散开：与原先一致，只做地形过滤，物理/ORCA 留给后续检查。
            if (options.Range > 0f &&
                TryGetRandomPoint(position, origin, options.Range, options.BoundaryMargin,
                    (candidate, sample) => IsRandomPointAllowed(sample, options),
                    out Vector3 random))
            {
                position = random;
                if (!TrySample(position, out origin)) return position;
            }

            if (IsPositionFree(position, origin, options) &&
                IsSpawnFiltered(position, origin, options, userData))
            {
                return position;
            }

            if (TrySearchPoint(position, origin,
                    (candidate, sample) =>
                        IsCandidateFree(candidate, sample, options) &&
                        IsSpawnFiltered(candidate, sample, options, userData),
                    out Vector3 point, out TerrainSample result))
            {
                return new Vector3(point.x,
                    result.IsWater ? result.WaterSurfaceY : result.SurfaceY,
                    point.z);
            }

            return position;
        }

        /// <summary>
        /// 依次执行已注册的出生点额外过滤器。
        /// </summary>
        private bool IsSpawnFiltered(Vector3 position, in TerrainSample sample,
            in SpawnPointOptions options, object userData)
        {
            for (int i = 0; i < filters.Count; i++)
            {
                if (!filters[i].IsSpawnPointFree(position, sample, options, userData))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 随机散开候选点过滤：水域与地表坡度（同区域、离边界距离由 TryGetRandomPoint 保证）。
        /// </summary>
        private static bool IsRandomPointAllowed(in TerrainSample sample,
            in SpawnPointOptions options)
        {
            if (!options.AllowWater && sample.IsWater) return false;
            if (options.RequireWalkable && !sample.Walkable &&
                !(options.AllowWater && sample.IsWater)) return false;
            return true;
        }

        /// <summary>
        /// 地形层面的出生点检查：坡度/埋入、水域。
        /// </summary>
        private static bool IsPositionFree(Vector3 position, in TerrainSample sample,
            in SpawnPointOptions options)
        {
            // 明显高于地表视为站在桥/平台上，地形约束交给其承托物。
            bool aboveTerrain = position.y > sample.SurfaceY + options.HeightTolerance;

            if (options.RequireWalkable)
            {
                // 允许入水时，水中的地形坡度不再作为限制。
                bool onTerrain = !aboveTerrain && !(options.AllowWater && sample.IsWater);
                if (onTerrain)
                {
                    if (!sample.Walkable) return false;
                    // 配置高度低于地表，视为埋入地形。
                    if (position.y < sample.SurfaceY - 0.05f) return false;
                }
            }

            // 高于水面（桥/平台上）不算入水。
            if (!options.AllowWater && sample.IsWater &&
                position.y <= sample.WaterSurfaceY + 0.1f) return false;

            return true;
        }

        /// <summary>
        /// 圆环搜索候选点过滤：水域、足印整体可行走、地形高度。
        /// </summary>
        private bool IsCandidateFree(Vector3 candidate, in TerrainSample sample,
            in SpawnPointOptions options)
        {
            if (!options.AllowWater && sample.IsWater) return false;

            // 只对陆地点要求足印整体可行走；允许入水时水中不要求地形坡度。
            if (options.RequireWalkable && !sample.IsWater && options.FootprintRadius > 0f &&
                !IsWalkable(candidate, options.FootprintRadius, out _))
            {
                return false;
            }

            var position = new Vector3(candidate.x,
                sample.IsWater ? sample.WaterSurfaceY : sample.SurfaceY,
                candidate.z);
            return IsPositionFree(position, sample, options);
        }

        /// <summary>
        /// 没有地形数据时的圆环搜索：只按已注册过滤器判定，高度保持不变。
        /// </summary>
        private Vector3 SearchWithoutTerrain(Vector3 position, in SpawnPointOptions options,
            object userData)
        {
            if (IsSpawnFiltered(position, default, options, userData)) return position;

            for (float radius = DefaultSearchStep; radius <= DefaultPhysicsSearchRadius;
                 radius += DefaultSearchStep)
            {
                for (int i = 0; i < DefaultSearchDirections; i++)
                {
                    float angle = (360f / DefaultSearchDirections) * i * Mathf.Deg2Rad;
                    var candidate = position +
                        new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
                    if (IsSpawnFiltered(candidate, default, options, userData)) return candidate;
                }
            }

            return position;
        }

        /// <summary>
        /// 在 <paramref name="origin"/> 所属区域内随机取点。随机半径按 origin 到区域边界的
        /// 距离收缩，并保证候选点与边界至少保留 <paramref name="boundaryMargin"/>，
        /// 避免散开到相邻区域或贴边。
        /// </summary>
        /// <param name="accept">候选点过滤；参数为候选点（Y 沿用 origin.y）与地形采样。</param>
        public bool TryGetRandomPoint(Vector3 origin, TerrainSample originSample, float range,
            float boundaryMargin, Func<Vector3, TerrainSample, bool> accept, out Vector3 point)
        {
            point = origin;
            if (range <= 0f) return false;

            float available = originSample.BoundaryDistance - boundaryMargin;
            if (available <= 0f) return false;
            float radius = Mathf.Min(range, available);

            for (int attempt = 0; attempt < DefaultRandomAttempts; attempt++)
            {
                var candidate = origin + Quaternion.Euler(0f, Random.Range(0f, 360f), 0f) *
                    Vector3.forward * Random.Range(0f, radius);
                if (!TrySample(candidate, out TerrainSample sample)) continue;
                if (sample.RegionId != originSample.RegionId) continue;
                if (sample.BoundaryDistance < boundaryMargin) continue;
                if (!accept(candidate, sample)) continue;

                point = candidate;
                return true;
            }

            return false;
        }

        /// <summary>
        /// 以 <paramref name="origin"/> 为中心按圆环逐层采样，候选点限制在 origin 所属区域内。
        /// 搜索半径取区域直径，区域半径未知时退回 <see cref="DefaultFallbackSearchRadius"/>。
        /// </summary>
        /// <param name="accept">候选点过滤；参数为候选点（Y 沿用 origin.y）与地形采样。</param>
        public bool TrySearchPoint(Vector3 origin, TerrainSample originSample,
            Func<Vector3, TerrainSample, bool> accept, out Vector3 point, out TerrainSample sample)
        {
            point = origin;
            sample = default;

            float maxRadius = originSample.RegionRadius > 0f
                ? originSample.RegionRadius * 2f
                : DefaultFallbackSearchRadius;

            for (float radius = DefaultSearchStep; radius <= maxRadius; radius += DefaultSearchStep)
            {
                for (int i = 0; i < DefaultSearchDirections; i++)
                {
                    float angle = (360f / DefaultSearchDirections) * i * Mathf.Deg2Rad;
                    var candidate = new Vector3(
                        origin.x + Mathf.Cos(angle) * radius,
                        origin.y,
                        origin.z + Mathf.Sin(angle) * radius);

                    if (!TrySample(candidate, out sample)) continue;
                    if (sample.RegionId != originSample.RegionId) continue;
                    if (!accept(candidate, sample)) continue;

                    point = candidate;
                    return true;
                }
            }

            return false;
        }
    }
}
