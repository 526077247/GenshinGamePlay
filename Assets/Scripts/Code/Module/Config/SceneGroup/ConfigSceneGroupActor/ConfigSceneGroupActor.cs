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
        
        public abstract Entity CreateActor(SceneGroup sceneGroup, float range);
    }
}