using ProtoBuf;

namespace TaoTie
{
    [ProtoContract]
    [ProtoInclude(100, typeof(ConfigCameraHardLookAtPlugin))]
    [ProtoInclude(101, typeof(ConfigCameraThirdPersonLookAtPlugin))]
    public abstract partial class ConfigCameraHeadPlugin: ConfigCameraPlugin
    {
        
    }
}