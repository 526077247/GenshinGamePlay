using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 可移动区域接口。用于约束实体的可移动范围。
    /// </summary>
    public interface IMoveArea
    {
        /// <summary>
        /// 判断世界坐标是否位于允许移动的区域内。
        /// </summary>
        bool Within(Vector3 pos);

        /// <summary>
        /// 将超出区域的坐标约束回区域内最近的合法位置。
        /// </summary>
        Vector3 ClampPosition(Vector3 pos);
    }
}