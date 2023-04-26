using UnityEngine;

namespace TaoTie
{
    public class FramingTransposerStateData
    {
        public Vector3 trackedObjectOffset;

        public float cameraDistance;

        public FramingTransposerStateData(ConfigFramingTransposer config)
        {
            if (config == null)
            {
                config = new ConfigFramingTransposer();
            }

            trackedObjectOffset = config.TrackedObjectOffset;
            cameraDistance = config.CameraDistance;
        }
    }
}