using System.Collections.Generic;
using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigCamera
    {

        [NinoMember(1)] [PropertyOrder(int.MinValue)] [Min(0)]
        public int Id;
#if UNITY_EDITOR
        [NinoMember(2)] [PropertyOrder(int.MinValue + 1)] [LabelText("策划备注")]
        public string Remarks;
#endif
        [NinoMember(3)] [Tooltip("更新朝向")] [BoxGroup("Plugin")]
        public ConfigCameraHeadPlugin HeadPlugin;

        [NinoMember(4)] [Tooltip("更新坐标")] [BoxGroup("Plugin")]
        public ConfigCameraBodyPlugin BodyPlugin;

        [NinoMember(5)] [Tooltip("其他后处理，如遮挡前推、震动等，顺序会影响最终效果")] [BoxGroup("Plugin")]
        public ConfigCameraOtherPlugin[] OtherPlugin;

        [NinoMember(6)] [Tooltip("相机入栈过渡混合动画")] [BoxGroup("Blender")]
        public ConfigBlender Enter;

        [NinoMember(7)] [Tooltip("相机出栈过渡混合动画")] [BoxGroup("Blender")]
        public ConfigBlender Leave;

        [NinoMember(8)] [Range(1, 179)] public float Fov = 90;

        [NinoMember(9)] [MinValue(0.01)] public float NearClipPlane = 0.3f;
        
        [NinoMember(10)] [MinValue(0.01)] public float FarClipPlane = 5000f;

        [NinoMember(11)] [LabelText("光标是否不锁定")] [BoxGroup("光标")]
        public bool UnLockCursor = true;

        [NinoMember(12)] [LabelText("显示光标")] [BoxGroup("光标")]
        public bool VisibleCursor = true;
    }
}