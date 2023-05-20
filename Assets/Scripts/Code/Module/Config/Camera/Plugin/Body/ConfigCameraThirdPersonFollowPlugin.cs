using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigCameraThirdPersonFollowPlugin : ConfigCameraBodyPlugin
    {
        [NinoMember(0)] [MinValue(1)] public int SpeedX = 100;

        [NinoMember(1)] [MinValue(1)] public int SpeedY = 20;

        [NinoMember(3)] [Range(-1, 20)] public float ZoomDefault = 2.5f;

        [NinoMember(4)] [LabelText("开启滚轮缩放")] [BoxGroup("滚轮缩放")]
        public bool EnableZoom;

        [NinoMember(5)] [ShowIf(nameof(EnableZoom))] [Range(-1, 20)] [BoxGroup("滚轮缩放")]
        public float ZoomMin = 1f;

        [NinoMember(6)] [ShowIf(nameof(EnableZoom))] [Range(-1, 20)] [BoxGroup("滚轮缩放")]
        public float ZoomMax = 4;

        [NinoMember(7)] [ShowIf(nameof(EnableZoom))] [Range(-1, 20)] [BoxGroup("滚轮缩放")]
        public float ZoomOffsetStart = 1.5f;

        [NinoMember(8)] [LabelText("近镜模式开关")] [BoxGroup("近镜模式")]
        public bool NearFocusEnable = true;

        [ShowIf(nameof(NearFocusEnable))]
        [BoxGroup("近镜模式")]
        [LabelText("近镜模式开始距离")]
        [Range(0.1f, 12f)]
        [NinoMember(9)]
        [Tooltip("从这个距离开始偏移")]
        public float NearFocusMaxDistance = 2f;

        [ShowIf(nameof(NearFocusEnable))]
        [BoxGroup("近镜模式")]
        [LabelText("近镜模式结束距离")]
        [Range(0.1f, 12f)]
        [Tooltip("这个距离达到偏移最大值")]
        [NinoMember(10)]
        public float NearFocusMinDistance = 1.5f;
    }
}