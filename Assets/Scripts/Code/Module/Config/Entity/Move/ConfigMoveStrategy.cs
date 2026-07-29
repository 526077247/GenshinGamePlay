using ProtoBuf;

namespace TaoTie
{
    [ProtoContract]
    [ProtoInclude(100, typeof(ConfigBulletMove))]
    [ProtoInclude(101, typeof(ConfigFollowMove))]
    [ProtoInclude(102, typeof(ConfigPlatformMove))]
    public abstract partial class ConfigMoveStrategy
    {
        
    }
}