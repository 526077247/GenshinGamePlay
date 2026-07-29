using System.Collections.Generic;
using ProtoBuf;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [ProtoContract]
    public partial class ConfigCamera
    {

        [ProtoMember(1)] [PropertyOrder(int.MinValue)] [MinValue(0)]
        public int Id;
#if UNITY_EDITOR
        [PropertyOrder(int.MinValue + 1)] [LabelText("策划备注")]
        public string Remarks;
#endif
        [ProtoMember(3)] [Tooltip("更新朝向")] [BoxGroup("Plugin")]
        public ConfigCameraHeadPlugin HeadPlugin;

        [ProtoMember(4)] [Tooltip("更新坐标")] [BoxGroup("Plugin")]
        public ConfigCameraBodyPlugin BodyPlugin;

        [ProtoMember(5)] [Tooltip("其他后处理，如遮挡前推、震动等，顺序会影响最终效果")] [BoxGroup("Plugin")]
        public ConfigCameraOtherPlugin[] OtherPlugin;

        [ProtoMember(6)] [Tooltip("相机入栈过渡混合动画")] [BoxGroup("Blender")]
        public ConfigBlender Enter;

        [ProtoMember(7)] [Tooltip("相机出栈过渡混合动画")] [BoxGroup("Blender")]
        public ConfigBlender Leave;

        [ProtoMember(8, IsRequired = true)] [Range(1, 179)] public float Fov = 90;

        [ProtoMember(9, IsRequired = true)] [MinValue(0.01)] public float NearClipPlane = 0.3f;
        
        [ProtoMember(10, IsRequired = true)] [MinValue(0.01)] public float FarClipPlane = 5000f;

        [ProtoMember(11, IsRequired = true)] [LabelText("光标是否不锁定")] [BoxGroup("光标")]
        public bool UnLockCursor = true;

        [ProtoMember(12, IsRequired = true)] [LabelText("显示光标")] [BoxGroup("光标")]
        public bool VisibleCursor = true;

        [ProtoMember(13)] [LabelText("用于角色面向")]
        public bool AvatarFaceDirection;
    }
}