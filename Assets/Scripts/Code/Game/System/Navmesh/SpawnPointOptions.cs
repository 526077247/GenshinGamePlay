namespace TaoTie
{
    /// <summary>
    /// 出生点求解参数。地形判定由 <see cref="TerrainQueryManager"/> 处理，
    /// 物理/ORCA 等额外限制通过回调传入。
    /// </summary>
    public struct SpawnPointOptions
    {
        /// <summary>随机散开半径，&lt;=0 表示不散开。</summary>
        public float Range;

        /// <summary>随机散开时与区域边界保留的最小距离。</summary>
        public float BoundaryMargin;

        /// <summary>是否允许出生在水里。</summary>
        public bool AllowWater;

        /// <summary>是否要求地表可行走（坡度限制）。</summary>
        public bool RequireWalkable;

        /// <summary>圆环搜索时的足印半径，&lt;=0 时跳过足印检查。</summary>
        public float FootprintRadius;

        /// <summary>出生高度高于地表该值时视为站在桥/平台等承托物上。</summary>
        public float HeightTolerance;
    }
}
