using Nino.Core;
using UnityEngine;

namespace TaoTie
{
    [NinoType(false)]
    public partial class ConfigCameraColliderPlugin: ConfigCameraOtherPlugin
    {
        [NinoMember(1)]
        public float Radius = 0.1f;

        [NinoMember(2)]
        public LayerMask CastLayer = LayerMask.GetMask("Default");
    }
}