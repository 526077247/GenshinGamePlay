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
        [ProtoMember(7)]
        [LabelText("出生是否允许在水里")]
        public bool AllowWaterSpawn;

        public Entity CreateActor(SceneGroup sceneGroup, float range)
        {
            var res = InnerCreateActor(sceneGroup, range);
            res.Position = AdjustSpawnPosition(res, res.ConfigActor?.Common, range);
            return res;
        }
        protected abstract Actor InnerCreateActor(SceneGroup sceneGroup, float range);

        #region 出生点避障

        /// <summary>
        /// 求解出生点：地形、水域由 <see cref="TerrainQueryManager"/> 判定，
        /// 物理碰撞、ORCA 等额外限制由注册到该管理器的 <see cref="ISpawnFilter"/> 处理。
        /// </summary>
        private Vector3 AdjustSpawnPosition(Actor actor, ConfigActorCommon common, float range)
        {
            if (common == null) return actor.Position;

            TerrainQueryManager terrain = TerrainQueryManager.Instance;
            if (terrain == null) return actor.Position;

            var options = new SpawnPointOptions
            {
                Range = range,
                //随机散开时与地形边界保留的最小距离，避免落在边缘。
                BoundaryMargin = 1,
                AllowWater = AllowWaterSpawn,
                RequireWalkable = IsSpawnAvoid,
                FootprintRadius = common.ModelRadius,
                //出生高度高于地表该值时视为站在桥/平台等承托物上，不再按脚下地形判定。
                HeightTolerance = 1,
            };

            return terrain.GetSpawnPosition(actor.Position, options, actor);
        }

        #endregion
    }
}