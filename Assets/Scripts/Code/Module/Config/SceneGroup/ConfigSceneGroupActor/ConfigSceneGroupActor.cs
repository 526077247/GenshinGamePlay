using System;
using TaoTie.LitJson.Extensions;
using ProtoBuf;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif
using UnityEngine;

namespace TaoTie
{
    [ProtoContract]
    [ProtoInclude(100, typeof(ConfigSceneGroupActorCharacter))]
    [ProtoInclude(101, typeof(ConfigSceneGroupActorGadget))]
    [ProtoInclude(102, typeof(ConfigSceneGroupActorMonster))]
    [ProtoInclude(103, typeof(ConfigSceneGroupActorAvatar))]
    public abstract class ConfigSceneGroupActor
    {
#if UNITY_EDITOR
        [PropertyOrder(int.MinValue+1)]
        [LabelText("策划备注")]
        public string Remarks;
#endif
        [ProtoMember(1)]
        [PropertyOrder(int.MinValue)]
        public int LocalId;
        [ProtoMember(2)]
        public Vector3 Position;
        [ProtoMember(3)]
        public Vector3 Rotation;
        [ProtoMember(4, IsRequired = true)][LabelText("是否是相对坐标、方向")]
        public bool IsLocal = true;
        [ProtoMember(5)]
#if UNITY_EDITOR
        [ValueDropdown("@"+nameof(OdinDropdownHelper)+"."+nameof(OdinDropdownHelper.GetCampTypeId)+"()")]
#endif
        public uint CampId;
        [ProtoMember(6)]
        [LabelText("出生是否避障")]
        public bool IsSpawnAvoid;

        public Entity CreateActor(SceneGroup sceneGroup, float range)
        {
            var res = InnerCreateActor(sceneGroup, range);
            res.Position = AdjustSpawnPosition(res.Position, res.ConfigActor?.Common);
            return res;
        }
        protected abstract Actor InnerCreateActor(SceneGroup sceneGroup, float range);

        #region 出生点避障

        /// <summary>
        /// 调整出生点以避开碰撞体障碍：目标位置空闲则直接使用，
        /// 否则在周围圆周采样一个空闲位置。position 为世界坐标。
        /// </summary>
        private Vector3 AdjustSpawnPosition(Vector3 position, ConfigActorCommon common)
        {
            if (!IsSpawnAvoid || common == null) return position;
            // 目标点没有碰撞体阻挡，直接使用
            if (IsPositionFree(position, common)) return position;

            // 在目标点周围圆周采样一个空闲位置
            return SearchFreePositionAround(position, common);
        }

        /// <summary>
        /// 检测世界坐标位置是否空闲（不被带碰撞体的障碍物占用）。
        /// </summary>
        private bool IsPositionFree(Vector3 worldPosition, ConfigActorCommon common)
        {
            return PhysicsHelper.OverlapSphereNonAlloc(worldPosition + Vector3.up * common.ModelHeight / 2,
                common.ModelRadius) == 0;
        }

        /// <summary>
        /// 在目标点周围按圆环比逐层采样，返回第一个无碰撞体的位置，找不到返回原位置。
        /// </summary>
        private Vector3 SearchFreePositionAround(Vector3 position, ConfigActorCommon common)
        {
            const float step = 1f;
            const int rings = 5;
            const int count = 12;
            for (int r = 1; r <= rings; r++)
            {
                float sampleRadius = r * step;
                for (int i = 0; i < count; i++)
                {
                    float angle = (360f / count) * i * Mathf.Deg2Rad;
                    var candidate = position + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * sampleRadius;
                    if (IsPositionFree(candidate, common))
                    {
                        return candidate;
                    }
                }
            }

            return position;
        }

        #endregion
    }
}