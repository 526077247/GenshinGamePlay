using ProtoBuf;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif
using UnityEngine;

namespace TaoTie
{
    [ProtoContract]
    public partial class ConfigCameraHardLockToTargetPlugin: ConfigCameraBodyPlugin
    {
        [ProtoMember(1)]
        public float Damping;
        [ProtoMember(2)]
        public Vector3 Offset;
        [ProtoMember(3, IsRequired = true)][LabelText("不跟随目标旋转")]
        public bool LockRotation = false;
        
        [ProtoMember(4)][ShowIf(nameof(LockRotation))]
        public Vector3 Rotation;
    }
}