namespace TaoTie
{
    /// <summary>
    /// 与具体地图实现无关的地形采样结果。
    /// </summary>
    public readonly struct TerrainSample
    {
        /// <summary>采样点是否位于地图/区域内。</summary>
        public readonly bool Valid;

        /// <summary>地表坡度是否在可行走范围内（不含水域判断）。</summary>
        public readonly bool Walkable;

        /// <summary>该点是否被水覆盖。</summary>
        public readonly bool IsWater;

        /// <summary>地表高度。</summary>
        public readonly float SurfaceY;

        /// <summary>水面高度（非水域时等于 <see cref="SurfaceY"/>）。</summary>
        public readonly float WaterSurfaceY;

        /// <summary>地表坡度（度）。</summary>
        public readonly float SlopeDegrees;

        /// <summary>所属区域标识。</summary>
        public readonly int RegionId;

        /// <summary>区域半径，用于限定局部搜索范围。</summary>
        public readonly float RegionRadius;

        /// <summary>到区域边界的最短水平距离。</summary>
        public readonly float BoundaryDistance;

        public TerrainSample(bool valid, bool walkable, bool isWater, float surfaceY,
            float waterSurfaceY, float slopeDegrees, int regionId, float regionRadius,
            float boundaryDistance)
        {
            Valid = valid;
            Walkable = walkable;
            IsWater = isWater;
            SurfaceY = surfaceY;
            WaterSurfaceY = waterSurfaceY;
            SlopeDegrees = slopeDegrees;
            RegionId = regionId;
            RegionRadius = regionRadius;
            BoundaryDistance = boundaryDistance;
        }

        /// <summary>地表可行走且不在水里。</summary>
        public bool IsWalkableGround => Valid && Walkable && !IsWater;

        /// <summary>
        /// 使用统一水面查询结果覆盖水域标记，其余地形字段保持不变。
        /// </summary>
        public TerrainSample WithWater(bool isWater)
        {
            if (isWater == IsWater) return this;
            return new TerrainSample(Valid, Walkable, isWater, SurfaceY, WaterSurfaceY,
                SlopeDegrees, RegionId, RegionRadius, BoundaryDistance);
        }
    }
}
