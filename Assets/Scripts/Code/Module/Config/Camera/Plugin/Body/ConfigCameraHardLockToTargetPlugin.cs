using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [NinoType(false)]
    public partial class ConfigCameraHardLockToTargetPlugin: ConfigCameraBodyPlugin
    {
        [NinoMember(1)]
        public float Damping;
        [NinoMember(2)]
        public Vector3 Offset;
        [NinoMember(3)][LabelText("不跟随目标旋转")]
        public bool LockRotation = false;
        
        [NinoMember(4)][ShowIf(nameof(LockRotation))]
        public Vector3 Rotation;
    }
}