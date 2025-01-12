using Nino.Core;

namespace TaoTie
{
    [NinoType(false)]
    public partial class ConfigCameraHardLockToTargetPlugin: ConfigCameraBodyPlugin
    {
        [NinoMember(1)]
        public float Damping;
    }
}