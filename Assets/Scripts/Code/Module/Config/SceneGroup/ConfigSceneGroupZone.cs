using System;
using System.Collections.Generic;
using ProtoBuf;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 区域
    /// </summary>
    [ProtoContract]
    public partial class ConfigSceneGroupZone
    {
#if UNITY_EDITOR
        [LabelText("策划备注")][PropertyOrder(int.MinValue+1)]
        public string Remarks;
#endif
        [ProtoMember(1)][PropertyOrder(int.MinValue)]
        public int LocalId;
        [ProtoMember(2)]
        public Vector3 Position;
        [ProtoMember(5)]
        public Vector3 Rotation;
        [ProtoMember(3, IsRequired = true)][LabelText("是否是相对坐标、方向")]
        public bool IsLocal = true;
        [ProtoMember(4)][NotNull]
        public ConfigShape Shape;

        public Zone CreateZone(SceneGroup sceneGroup)
        {
            var entity = sceneGroup.Parent.CreateEntity<Zone, ConfigShape>(Shape);
            Vector3 position;
            Quaternion rotation;
            if (IsLocal)
            {
                position = Quaternion.Euler(sceneGroup.Rotation) * Position + sceneGroup.Position;
                rotation = Quaternion.Euler(sceneGroup.Rotation + Rotation);
            }
            else
            {
                position = Position;
                rotation = Quaternion.Euler(Rotation);
            }
            entity.Rotation = rotation;
            entity.Position = position;
            entity.AddComponent<SceneGroupZoneComponent, int, long>(LocalId, sceneGroup.Id);
            return entity;
        }
    }
}