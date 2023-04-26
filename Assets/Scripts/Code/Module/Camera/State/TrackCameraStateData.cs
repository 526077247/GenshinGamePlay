using Nino.Serialization;
using UnityEngine;

namespace TaoTie
{
    [NinoSerialize]
    public class TrackCameraStateData : CameraStateData
    {
        public TrackedDollyStateData TrackedDolly;

        public CameraSmoothRoute SmoothRoute;

        public float Progress = 0;

        public Transform FocusTrans;

        //当focusTrans==null时有效
        public Vector3 FocusPosition;

        public TrackCameraStateData()
        {
        }

        public TrackCameraStateData(ConfigTrackCamera config) : base(config)
        {
            TrackedDolly = new TrackedDollyStateData(config.TrackedDolly);
        }
    }
}