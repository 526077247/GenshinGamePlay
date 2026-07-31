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
    public partial class ConfigCameraShake
    {
        [ProtoMember(2)][LabelText("震动方向类型")]
        public CameraShakeType ShakeType; 
        [ProtoMember(1)][LabelText("击中才广播")][ShowIf("@"+nameof(ShakeType)+"!="+nameof(CameraShakeType)+"."+nameof(CameraShakeType.HitVector))]
        public bool BroadcastOnHit;
        [ProtoMember(3)][LabelText("震动幅度")]
        public float ShakeRange;
        [ProtoMember(4)][LabelText("震动时间")]
        public int ShakeTime;
        [ProtoMember(5)][LabelText("震动事件广播距离")]
        public float ShakeDistance;
        [ProtoMember(6)][LabelText("震动频率")]
        public int ShakeFrequency;
        [ProtoMember(7)][ShowIf(nameof(ShakeType),CameraShakeType.CustomVector)][LabelText("震动方向")]
        public Vector3 ShakeDir;
        [ProtoMember(8)][LabelText("衰减范围")]
        public float RangeAttenuation;
    }
}