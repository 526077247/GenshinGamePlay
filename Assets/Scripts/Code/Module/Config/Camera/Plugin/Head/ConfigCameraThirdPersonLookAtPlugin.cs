using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [NinoType(false)]
    public partial class ConfigCameraThirdPersonLookAtPlugin: ConfigCameraHeadPlugin
    {
        [NinoMember(1)] [LabelText("近镜模式开关")] [BoxGroup("近镜模式")]
        public bool NearFocusEnable = true;

        [ShowIf(nameof(NearFocusEnable))]
        [BoxGroup("近镜模式")]
        [LabelText("近镜模式开始距离")]
        [Range(0.1f, 12f)]
        [NinoMember(2)]
        [Tooltip("从这个距离开始偏移")]
        public float NearFocusMaxDistance = 2f;

        [ShowIf(nameof(NearFocusEnable))]
        [BoxGroup("近镜模式")]
        [LabelText("近镜模式结束距离")]
        [Range(0.1f, 12f)]
        [Tooltip("这个距离达到偏移最大值")]
        [NinoMember(3)]
        public float NearFocusMinDistance = 1.5f;
    }
}