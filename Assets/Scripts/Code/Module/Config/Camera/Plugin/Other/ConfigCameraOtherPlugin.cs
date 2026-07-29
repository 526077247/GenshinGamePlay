using ProtoBuf;

namespace TaoTie
{
    [ProtoContract]
    [ProtoInclude(100, typeof(ConfigCameraColliderPlugin))]
    [ProtoInclude(101, typeof(ConfigCameraShakePlugin))]
    public abstract partial class ConfigCameraOtherPlugin: ConfigCameraPlugin
    {
        
    }
}