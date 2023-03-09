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
        [NinoMember(20)] public CinemachineBodyType _body;

        [ShowIf(nameof(_body), CinemachineBodyType.HardLockToTarget)] 
        [NinoMember(21)]public ConfigHardLockToTarget _hardLockToTarget;

        [ShowIf(nameof(_body), CinemachineBodyType.Transposer)] 
        [NinoMember(22)]public ConfigTransposer _transposer;

        [ShowIf(nameof(_body), CinemachineBodyType.FramingTransposer)] 
        [NinoMember(23)]public ConfigFramingTransposer _framingTransposer;
    }
}