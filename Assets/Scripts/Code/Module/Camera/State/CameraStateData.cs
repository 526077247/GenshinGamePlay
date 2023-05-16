using System;
using UnityEngine;
namespace TaoTie
{
    public class CameraStateData: IDisposable
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

        public static CameraStateData Create()
        {
            return ObjectPool.Instance.Fetch<CameraStateData>();
        }
        
        public static CameraStateData DeepClone(CameraStateData other)
        {
            var res = ObjectPool.Instance.Fetch<CameraStateData>();
            res.Fov = other.Fov;
            res.NearClipPlane = other.Fov;
            res.Dutch = other.Fov;
            res.Up = other.Up;
            res.Position = other.Position;
            res.Orientation = other.Orientation;
            res.Spherical = other.Spherical;
            res.LookAt = other.LookAt;
            res.TargetForward = other.TargetForward;
            res.Forward = other.Forward;
            res.ForwardPoleDeltaAngle = other.ForwardPoleDeltaAngle;
            res.ForwardElevDeltaAngle = other.ForwardElevDeltaAngle;
            return res;
        }
        public void Dispose()
        {
            Fov = default;
            NearClipPlane = default;
            Dutch = default;
            Up = default;
            Position = default;
            Orientation = default;
            Spherical = default;
            LookAt = default;
            TargetForward = default;
            Forward = default;
            ForwardPoleDeltaAngle = default;
            ForwardElevDeltaAngle = default;
            ObjectPool.Instance.Recycle(this);
        }
    }
}