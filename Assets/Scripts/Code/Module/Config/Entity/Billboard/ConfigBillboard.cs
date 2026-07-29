using ProtoBuf;
using UnityEngine;

namespace TaoTie
{
    [ProtoContract]
    public partial class ConfigBillboard
    {
        [ProtoMember(1)]
        public string AttachPoint;
        [ProtoMember(2)]
        public Vector3 Offset;
        [ProtoMember(3)]
        public ConfigBillboardPlugin[] Plugins;
    }
}