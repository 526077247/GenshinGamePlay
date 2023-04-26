namespace TaoTie
{
    public class FreeLookCameraStateData : CameraStateData
    {
        public float[] Height;
        public float NearFocusMaxDistance;
        public float NearFocusMinDistance;
        public bool NearFocusEnable;
        public float[] Radius;
        public float XSpeed;
        public float YSpeed;

        public FreeLookCameraStateData()
        {
        }

        public FreeLookCameraStateData(ConfigFreeLookCamera config) : base(config)
        {
            Height = config.Height;
            Radius = config.Radius;
            YSpeed = config.YSpeed;
            XSpeed = config.XSpeed;
            NearFocusEnable = config.NearFocusEnable;
            NearFocusMaxDistance = config.NearFocusMaxDistance;
            NearFocusMinDistance = config.NearFocusMinDistance;
        }
    }
}