using Nino.Serialization;
using UnityEngine;

namespace TaoTie
{
    [NinoSerialize]
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