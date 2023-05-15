using System.Collections.Generic;
using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigCamera
    {

        [NinoMember(1)][PropertyOrder(int.MinValue)] [Min(0)]
        public int Id;
#if UNITY_EDITOR
        [NinoMember(2)][PropertyOrder(int.MinValue + 1)] [LabelText("策划备注")]
        public string Remarks;
#endif
        [NinoMember(3)] [Tooltip("更新朝向")] [TabGroup("Plugin")]
        public ConfigCameraHeadPlugin HeadPlugin;
        
        [NinoMember(4)] [Tooltip("更新坐标")][TabGroup("Plugin")]
        public ConfigCameraBodyPlugin BodyPlugin;

        [NinoMember(5)] [Tooltip("其他后处理，如遮挡前推、震动等，顺序会影响最终效果")][TabGroup("Plugin")]
        public ConfigCameraOtherPlugin[] OtherPlugin;

        [NinoMember(6)] [Tooltip("相机入栈过渡混合动画")][TabGroup("Bleed")]
        public ConfigCameraBleed Enter;

        [NinoMember(7)] [Tooltip("相机出栈过渡混合动画")][TabGroup("Bleed")]
        public ConfigCameraBleed Level;
    }
}