using System.Collections.Generic;

using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    public class ConfigVirtualCamera : ConfigCamera
    {
        public override CameraType type => CameraType.VirtualCameraPlugin;
        [SerializeField] private CinemachineBodyType _body;

        [ShowIf(nameof(_body), CinemachineBodyType.HardLockToTarget)] [SerializeField]
        private ConfigHardLockToTarget _hardLockToTarget;

        [ShowIf(nameof(_body), CinemachineBodyType.Transposer)] [SerializeField]
        private ConfigTransposer _transposer;

        [ShowIf(nameof(_body), CinemachineBodyType.FramingTransposer)] [SerializeField]
        private ConfigFramingTransposer _framingTransposer;


        public CinemachineBodyType body => _body;
        public ConfigHardLockToTarget hardLockToTarget => _hardLockToTarget;
        public ConfigTransposer transposer => _transposer;

        public ConfigFramingTransposer framingTransposer => _framingTransposer;
    }
}