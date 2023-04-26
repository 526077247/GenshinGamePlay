using System.Collections.Generic;
using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigVirtualCamera : ConfigCamera
    {
        public override CameraType Type => CameraType.VirtualCameraPlugin;
        [NinoMember(20)] public CinemachineBodyType Body;

        [ShowIf(nameof(Body), CinemachineBodyType.HardLockToTarget)] 
        [NinoMember(21)]public ConfigHardLockToTarget HardLockToTarget;

        [ShowIf(nameof(Body), CinemachineBodyType.Transposer)] 
        [NinoMember(22)]public ConfigTransposer Transposer;

        [ShowIf(nameof(Body), CinemachineBodyType.FramingTransposer)] 
        [NinoMember(23)]public ConfigFramingTransposer FramingTransposer;
    }
}