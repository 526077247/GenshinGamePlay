using Nino.Serialization;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigCameraHardLockToTargetPlugin: ConfigCameraBodyPlugin
    {
        [NinoMember(0)]
        public float Damping;
    }
}