using ProtoBuf;

namespace TaoTie
{
    [ProtoContract]
    [ProtoInclude(100, typeof(ConfigRotateAroundArrange))]
    public abstract partial class ConfigArrange
    {
        [ProtoMember(1)]
        public float Damping;
    }
}