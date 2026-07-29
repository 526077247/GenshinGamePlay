using ProtoBuf;
using UnityEngine;

namespace TaoTie
{
    [ProtoContract]
    public partial class ConfigCameraColliderPlugin: ConfigCameraOtherPlugin
    {
        [ProtoMember(1, IsRequired = true)]
        public float Radius = 0.1f;

        [ProtoMember(2, IsRequired = true)]
        public LayerMask CastLayer = LayerMask.GetMask("Default");
    }
}