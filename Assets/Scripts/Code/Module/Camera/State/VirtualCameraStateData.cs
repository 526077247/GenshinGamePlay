using UnityEngine;

namespace TaoTie
{
    public class VirtualCameraStateData : CameraStateData
    {
        public CinemachineBodyType body;
        public Vector3 follow;
        public FramingTransposerStateData framingTransposer;
        public HardLockToTargetStateData hardLockToTarget;
        public Vector3 lookAt;
        public TransposerStateData transposer;
        public bool cut = false;
        public VirtualCameraStateData()
        {
        }

        public VirtualCameraStateData(ConfigVirtualCamera config) : base(config)
        {
            body = config.body;
            if (body == CinemachineBodyType.HardLockToTarget)
                hardLockToTarget = new HardLockToTargetStateData(config.hardLockToTarget);
            else if (body == CinemachineBodyType.Transposer)
                transposer = new TransposerStateData(config.transposer);
            else if (body == CinemachineBodyType.FramingTransposer)
                framingTransposer = new FramingTransposerStateData(config.framingTransposer);
        }
    }
}