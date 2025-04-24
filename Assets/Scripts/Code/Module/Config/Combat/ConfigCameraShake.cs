using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [NinoType(false)]
    public partial class ConfigCameraShake
    {
        [NinoMember(2)][LabelText("震动方向类型")]
        public CameraShakeType ShakeType; 
        [NinoMember(1)][LabelText("击中才广播")][ShowIf("@"+nameof(ShakeType)+"!="+nameof(CameraShakeType)+"."+nameof(CameraShakeType.HitVector))]
        public bool BroadcastOnHit;
        [NinoMember(3)][LabelText("震动幅度")]
        public float ShakeRange;
        [NinoMember(4)][LabelText("震动时间")]
        public int ShakeTime;
        [NinoMember(5)][LabelText("震动事件广播距离")]
        public float ShakeDistance;
        [NinoMember(6)][LabelText("震动频率")]
        public int ShakeFrequency;
        [NinoMember(7)][ShowIf(nameof(ShakeType),CameraShakeType.CustomVector)][LabelText("震动方向")]
        public Vector3 ShakeDir;
        [NinoMember(8)][LabelText("衰减范围")]
        public float RangeAttenuation;
    }
}