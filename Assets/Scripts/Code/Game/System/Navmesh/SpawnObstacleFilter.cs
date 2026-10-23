using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 出生点物理侧过滤：碰撞体、水域 Collider 与 ORCA 静态障碍。
    /// 注册到 <see cref="TerrainQueryManager"/> 后对所有出生点求解生效。
    /// </summary>
    public sealed class SpawnObstacleFilter : ISpawnFilter
    {
        private static readonly int waterLayerMask = LayerMask.GetMask("Water");

        public bool IsSpawnPointFree(Vector3 position, in TerrainSample sample,
            in SpawnPointOptions options, object userData)
        {
            if (!(userData is Actor actor)) return true;

            ConfigActorCommon common = actor.ConfigActor?.Common;
            if (common == null) return true;

            if (options.RequireWalkable &&
                PhysicsHelper.OverlapSphereNonAlloc(
                    position + Vector3.up * common.ModelHeight / 2, common.ModelRadius) != 0)
            {
                return false;
            }

            if (!options.AllowWater && IsInWater(position, common)) return false;

            if (options.RequireWalkable)
            {
                ORCAAgentComponent orca = actor.GetComponent<ORCAAgentComponent>();
                if (orca != null && orca.IsRVOEnabled &&
                    orca.OverlapsObstacle(position, common.ModelRadius, position.y,
                        common.ModelHeight))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 检测世界坐标位置是否被水面/河流覆盖（基于 Water 层的 MeshCollider）。
        /// 用胶囊覆盖从脚下到头顶的竖向范围，避免高模型中心球体够不到水面。
        /// </summary>
        private static bool IsInWater(Vector3 worldPosition, ConfigActorCommon common)
        {
            float r = common.ModelRadius;
            Vector3 bottom = worldPosition + Vector3.up * r;
            Vector3 top = worldPosition + Vector3.up * Mathf.Max(common.ModelHeight - r, r);
            return Physics.CheckCapsule(bottom, top, r, waterLayerMask, QueryTriggerInteraction.Ignore);
        }
    }
}
