using ProtoBuf;

namespace TaoTie
{
    [ProtoContract]
    [ProtoInclude(100, typeof(ConfigAnimatorMove))]
    [ProtoInclude(101, typeof(ConfigRigidbodyMove))]
    [ProtoInclude(102, typeof(ConfigSimpleMove))]
    public abstract partial class ConfigMoveAgent
    {
        
    }
}