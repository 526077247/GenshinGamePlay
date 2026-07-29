using ProtoBuf;

namespace TaoTie
{
    [ProtoContract]
    [ProtoInclude(100, typeof(ConfigCameraHardLockToTargetPlugin))]
    [ProtoInclude(101, typeof(ConfigCameraThirdPersonFollowPlugin))]
    public abstract partial class ConfigCameraBodyPlugin: ConfigCameraPlugin
    {
        
    }
}