using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 出生点额外限制（物理碰撞、ORCA、禁放区等）。实现后注册到
    /// <see cref="TerrainQueryManager"/>，由出生点求解流程统一调用。
    /// </summary>
    public interface ISpawnFilter
    {
        /// <summary>
        /// 判断候选点是否满足额外限制。
        /// </summary>
        /// <param name="userData">调用方传入的上下文（通常为 Actor），可为空。</param>
        bool IsSpawnPointFree(Vector3 position, in TerrainSample sample,
            in SpawnPointOptions options, object userData);
    }
}
