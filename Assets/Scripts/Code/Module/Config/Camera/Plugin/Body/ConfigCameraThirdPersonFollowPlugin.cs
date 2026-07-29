using ProtoBuf;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [ProtoContract]
    public partial class ConfigCameraThirdPersonFollowPlugin : ConfigCameraBodyPlugin
    {
        [ProtoMember(1, IsRequired = true)] [MinValue(1)] public int SpeedX = 100;

        [ProtoMember(2, IsRequired = true)] [MinValue(1)] public int SpeedY = 20;

        [ProtoMember(3, IsRequired = true)] [Range(-1, 20)] public float ZoomDefault = 2.5f;

        [ProtoMember(4)] [LabelText("开启滚轮缩放")] [BoxGroup("滚轮缩放")]
        public bool EnableZoom;

        [ProtoMember(5, IsRequired = true)] [ShowIf(nameof(EnableZoom))] [Range(-1, 20)] [BoxGroup("滚轮缩放")]
        public float ZoomMin = 1f;

        [ProtoMember(6, IsRequired = true)] [ShowIf(nameof(EnableZoom))] [Range(-1, 20)] [BoxGroup("滚轮缩放")]
        public float ZoomMax = 4;
    }
}