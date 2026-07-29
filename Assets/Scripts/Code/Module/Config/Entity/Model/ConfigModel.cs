using ProtoBuf;

namespace TaoTie
{
    [ProtoContract]
    [ProtoInclude(100, typeof(ConfigMultiModel))]
    [ProtoInclude(101, typeof(ConfigSingletonModel))]
    public abstract partial class ConfigModel
    {

    }
}