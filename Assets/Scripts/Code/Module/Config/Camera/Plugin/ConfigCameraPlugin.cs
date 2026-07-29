using ProtoBuf;

namespace TaoTie
{
    [ProtoContract]
    [ProtoInclude(100, typeof(ConfigCameraBodyPlugin))]
    [ProtoInclude(104, typeof(ConfigCameraHeadPlugin))]
    [ProtoInclude(107, typeof(ConfigCameraOtherPlugin))]
    public abstract class ConfigCameraPlugin
    {
        
    }
}