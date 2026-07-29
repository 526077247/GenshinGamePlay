using ProtoBuf;
using UnityEngine;

namespace TaoTie
{
    [ProtoContract]
    public partial class ConfigFsmTimeline
    {
        [ProtoMember(1)]
        public ConfigFsmClip[] Clips;
    }
}