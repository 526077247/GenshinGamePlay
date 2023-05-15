using UnityEngine;
namespace TaoTie
{
    public class CameraStateData
    {
        public float Fov; 
        public float NearClipPlane; 
        public float Dutch; 
        public Vector3 Up;
        public Vector3 Position;
        public Quaternion Orientation;
        public Vector3 Spherical;
        public Vector3 LookAt;
        public Vector3 TargetForward;
        public Vector3 Forward;
        public float ForwardPoleDeltaAngle;
        public float ForwardElevDeltaAngle;
    }
}