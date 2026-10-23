using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 地形/可行走查询接口。由具体地图实现注册到 <see cref="TerrainQueryManager"/>，
    /// 玩法侧只依赖该接口，不感知具体地图类型。
    /// </summary>
    public interface ITerrainQuery
    {
        /// <summary>
        /// 采样世界坐标处的地形信息。
        /// </summary>
        bool TrySample(Vector3 worldPosition, out TerrainSample sample);

        /// <summary>
        /// 判断以 <paramref name="worldPosition"/> 为圆心、<paramref name="radius"/> 为半径的
        /// 足印是否整体位于可行走地表上。
        /// </summary>
        bool IsWalkable(Vector3 worldPosition, float radius, out TerrainSample center);
    }
}
