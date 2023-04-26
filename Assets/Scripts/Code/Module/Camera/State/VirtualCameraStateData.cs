using UnityEngine;

namespace TaoTie
{
    public class VirtualCameraStateData : CameraStateData
    {
        public CinemachineBodyType Body;
        public Vector3 Follow;
        public FramingTransposerStateData FramingTransposer;
        public HardLockToTargetStateData HardLockToTarget;
        public Vector3 LookAt;
        public TransposerStateData Transposer;
        public bool Cut = false;
        public VirtualCameraStateData()
        {
        }

        public VirtualCameraStateData(ConfigVirtualCamera config) : base(config)
        {
            Body = config.Body;
            if (Body == CinemachineBodyType.HardLockToTarget)
                HardLockToTarget = new HardLockToTargetStateData(config.HardLockToTarget);
            else if (Body == CinemachineBodyType.Transposer)
                Transposer = new TransposerStateData(config.Transposer);
            else if (Body == CinemachineBodyType.FramingTransposer)
                FramingTransposer = new FramingTransposerStateData(config.FramingTransposer);
        }
    }
}