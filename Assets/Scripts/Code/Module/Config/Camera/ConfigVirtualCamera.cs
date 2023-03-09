using System.Collections.Generic;
using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigVirtualCamera : ConfigCamera
    {
        public override CameraType type => CameraType.VirtualCameraPlugin;
        [NinoMember(20)] public CinemachineBodyType body;

        [ShowIf(nameof(body), CinemachineBodyType.HardLockToTarget)] 
        [NinoMember(21)]public ConfigHardLockToTarget hardLockToTarget;

        [ShowIf(nameof(body), CinemachineBodyType.Transposer)] 
        [NinoMember(22)]public ConfigTransposer transposer;

        [ShowIf(nameof(body), CinemachineBodyType.FramingTransposer)] 
        [NinoMember(23)]public ConfigFramingTransposer framingTransposer;
    }
}