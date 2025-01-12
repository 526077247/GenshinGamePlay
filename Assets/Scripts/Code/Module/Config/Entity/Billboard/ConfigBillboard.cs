using Nino.Core;
using UnityEngine;

namespace TaoTie
{
    [NinoType(false)]
    public partial class ConfigBillboard
    {
        [NinoMember(1)]
        public string AttachPoint;
        [NinoMember(2)]
        public Vector3 Offset;
        [NinoMember(3)]
        public ConfigBillboardPlugin[] Plugins;
    }
}