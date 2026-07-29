using System.Collections.Generic;
using ProtoBuf;

namespace TaoTie
{
    [ProtoContract]
    public partial class ConfigHitScene
    {
        [ProtoMember(1)]
        public string DefaultEffect;
        [ProtoMember(2, IsRequired = true)]
        public Dictionary<string, string> SurfaceEffect = new Dictionary<string, string>();
    }
}