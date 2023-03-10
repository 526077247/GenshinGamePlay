// using UnityEngine;
//
// namespace TaoTie
// {
//     public class TrackCameraStateData : CameraStateData
//     {
//         public TrackedDollyStateData trackedDolly;
//
//         public CameraSmoothRoute smoothRoute;
//
//         public float progress = 0;
//
//         public Transform focusTrans;
//
//         //当focusTrans==null时有效
//         public Vector3 focusPosition;
//
//         public TrackCameraStateData()
//         {
//         }
//
//         public TrackCameraStateData(ConfigTrackCamera config) : base(config)
//         {
//             trackedDolly = new TrackedDollyStateData(config.trackedDolly);
//         }
//     }
// }