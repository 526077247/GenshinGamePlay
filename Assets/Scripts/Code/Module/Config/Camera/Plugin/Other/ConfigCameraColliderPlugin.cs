using Nino.Serialization;
using UnityEngine;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigCameraColliderPlugin: ConfigCameraOtherPlugin
    {
        [NinoMember(1)]
        public float Radius = 0.1f;

        [NinoMember(2)]
        public LayerMask CastLayer = LayerMask.GetMask("Default");
    }
}