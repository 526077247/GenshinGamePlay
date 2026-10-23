using UnityEngine;

namespace TaoTie
{
    public sealed class NavmeshTerrainQuery : ITerrainQuery, IWaterQuery
    {
        public const float DefaultMaxWalkableSlopeDegrees = 30f;
        public const float DefaultMaxSurfaceDistance = 2f;
        public const float DefaultWaterProbeHeight = 2f;

        private const float SlopeSampleDelta = 0.25f;
        private const float WaterProbeRadius = 0.1f;
        private const int FootprintSamples = 8;

        private static readonly int DefaultWaterLayerMask = LayerMask.GetMask("Water");

        private readonly string navMeshName;
        private readonly float maxSlopeDegrees;
        private readonly float maxSurfaceDistance;
        private readonly int waterLayerMask;
        private readonly float waterProbeHeight;

        public NavmeshTerrainQuery(string navMeshName,
            float maxSlopeDegrees = DefaultMaxWalkableSlopeDegrees,
            float maxSurfaceDistance = DefaultMaxSurfaceDistance,
            int waterLayerMask = -1,
            float waterProbeHeight = DefaultWaterProbeHeight)
        {
            this.navMeshName = navMeshName;
            this.maxSlopeDegrees = Mathf.Clamp(maxSlopeDegrees, 0f, 89f);
            this.maxSurfaceDistance = Mathf.Max(0f, maxSurfaceDistance);
            this.waterLayerMask = waterLayerMask < 0 ? DefaultWaterLayerMask : waterLayerMask;
            this.waterProbeHeight = Mathf.Max(0.1f, waterProbeHeight);
        }

        public string NavMeshName => navMeshName;

        public bool TrySample(Vector3 worldPosition, out TerrainSample sample)
        {
            sample = default;
            if (!TryGetSurface(worldPosition, out Vector3 surface, out float boundaryDistance))
                return false;

            bool isWater = IsWater(surface);
            float slope = SampleSlopeDegrees(worldPosition);
            sample = new TerrainSample(true, !isWater && slope <= maxSlopeDegrees, isWater,
                surface.y, surface.y, slope, 0, 0f, boundaryDistance);
            return true;
        }

        public bool IsWalkable(Vector3 worldPosition, float radius, out TerrainSample center)
        {
            if (!TrySample(worldPosition, out center)) return false;
            if (!center.Walkable) return false;
            if (radius <= 0f) return true;

            for (int i = 0; i < FootprintSamples; i++)
            {
                float angle = i * (Mathf.PI * 2f / FootprintSamples);
                Vector3 p = worldPosition +
                            new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
                if (!TrySample(p, out TerrainSample ring) || !ring.Walkable)
                    return false;
            }

            return true;
        }

        public bool TryGetWater(Vector3 worldPosition, out bool isWater)
        {
            isWater = false;
            if (!TryGetSurface(worldPosition, out Vector3 surface, out _)) return false;
            isWater = IsWater(surface);
            return true;
        }

        public bool TryOverlapsWater(Vector3 worldPosition, float radius, float baseline,
            float height, out bool overlaps)
        {
            overlaps = false;
            if (!TryGetSurface(worldPosition, out Vector3 surface, out _)) return false;
            if (IsWater(surface))
            {
                overlaps = true;
                return true;
            }

            if (waterLayerMask == 0) return true;

            float r = Mathf.Max(radius, WaterProbeRadius);
            Vector3 bottom = new(worldPosition.x, baseline + r, worldPosition.z);
            Vector3 top = new(worldPosition.x, baseline + Mathf.Max(height, r), worldPosition.z);
            overlaps = Physics.CheckCapsule(bottom, top, r, waterLayerMask,
                QueryTriggerInteraction.Ignore);
            return true;
        }

        private bool TryGetSurface(Vector3 worldPosition, out Vector3 surface,
            out float boundaryDistance)
        {
            surface = worldPosition;
            boundaryDistance = 0f;
            if (NavmeshSystem.Instance == null) return false;
            if (!NavmeshSystem.Instance.TryGetNearestPoly(navMeshName, worldPosition,
                    out long polyRef, out Vector3 nearest, out bool overPoly))
            {
                return false;
            }

            float distance = new Vector2(worldPosition.x - nearest.x, worldPosition.z - nearest.z)
                .magnitude;
            if (!overPoly && distance > maxSurfaceDistance) return false;
            if (Mathf.Abs(worldPosition.y - nearest.y) > maxSurfaceDistance) return false;

            if (NavmeshSystem.Instance.TryGetPolyHeight(navMeshName, polyRef, nearest,
                    out float height))
            {
                nearest.y = height;
            }

            if (NavmeshSystem.Instance.TryGetPolyBoundary(navMeshName, polyRef, worldPosition,
                    out Vector3 boundary))
            {
                boundaryDistance =
                    new Vector2(worldPosition.x - boundary.x, worldPosition.z - boundary.z).magnitude;
            }
            else
            {
                boundaryDistance = maxSurfaceDistance;
            }

            surface = nearest;
            return true;
        }

        private float SampleSlopeDegrees(Vector3 worldPosition)
        {
            float dx = SlopeSampleDelta;
            if (!TryGetSurface(worldPosition + new Vector3(dx, 0f, 0f), out Vector3 px, out _) ||
                !TryGetSurface(worldPosition - new Vector3(dx, 0f, 0f), out Vector3 nx, out _) ||
                !TryGetSurface(worldPosition + new Vector3(0f, 0f, dx), out Vector3 pz, out _) ||
                !TryGetSurface(worldPosition - new Vector3(0f, 0f, dx), out Vector3 nz, out _))
            {
                return 90f;
            }

            float gx = (px.y - nx.y) / (2f * dx);
            float gz = (pz.y - nz.y) / (2f * dx);
            return Mathf.Atan(Mathf.Sqrt(gx * gx + gz * gz)) * Mathf.Rad2Deg;
        }

        private bool IsWater(Vector3 surface)
        {
            if (waterLayerMask == 0) return false;
            Vector3 bottom = surface + Vector3.up * 0.05f;
            Vector3 top = surface + Vector3.up * waterProbeHeight;
            return Physics.CheckCapsule(bottom, top, WaterProbeRadius, waterLayerMask,
                QueryTriggerInteraction.Ignore);
        }
    }
}
