using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 水面查询接口。可分别注册，玩法侧只依赖该接口。
    /// 返回 false 表示该查询不负责此位置，继续尝试下一个查询。
    /// </summary>
    public interface IWaterQuery
    {
        /// <summary>
        /// 查询单点是否在水面内。
        /// </summary>
        bool TryGetWater(Vector3 worldPosition, out bool isWater);

        /// <summary>
        /// 查询水平足印是否与水面重叠。
        /// </summary>
        bool TryOverlapsWater(Vector3 worldPosition, float radius, float baseline, float height,
            out bool overlaps);
    }
}
