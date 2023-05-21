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
    }
}