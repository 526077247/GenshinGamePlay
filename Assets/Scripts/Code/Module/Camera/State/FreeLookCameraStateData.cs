namespace TaoTie
{
    public class FreeLookCameraStateData : CameraStateData
    {
        public float[] height;
        public float nearFocusMaxDistance;
        public float nearFocusMinDistance;
        public bool nearFocusEnable;
        public float[] radius;
        public float xSpeed;
        public float ySpeed;

        public FreeLookCameraStateData()
        {
        }

        public FreeLookCameraStateData(ConfigFreeLookCamera config) : base(config)
        {
            height = config.height;
            radius = config.radius;
            ySpeed = config.ySpeed;
            xSpeed = config.xSpeed;
            nearFocusEnable = config.nearFocusEnable;
            nearFocusMaxDistance = config.nearFocusMaxDistance;
            nearFocusMinDistance = config.nearFocusMinDistance;
        }
    }
}