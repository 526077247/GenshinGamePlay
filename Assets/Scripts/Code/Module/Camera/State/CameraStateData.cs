using System;
using UnityEngine;
namespace TaoTie
{
    public class CameraStateData: IDisposable
    {
        public float Fov; 
        public float NearClipPlane;
        
        public Vector3 Up;
        public Vector3 Forward;
        
        public Vector3 Position;
        public Quaternion Orientation;
        
        public Vector3 LookAt;
        public Vector3 TargetForward;

        public static CameraStateData Create()
        {
            return ObjectPool.Instance.Fetch<CameraStateData>();
        }
        
        public static CameraStateData DeepClone(CameraStateData other)
        {
            var res = ObjectPool.Instance.Fetch<CameraStateData>();
            res.Fov = other.Fov;
            res.NearClipPlane = other.NearClipPlane;
            res.Up = other.Up;
            res.Forward = other.Forward;
            res.Position = other.Position;
            res.Orientation = other.Orientation;
            res.LookAt = other.LookAt;
            res.TargetForward = other.TargetForward;
           
            return res;
        }
        public void Dispose()
        {
            Fov = default;
            NearClipPlane = default;
            Up = default;
            Forward = default;
            Position = default;
            Orientation = default;
            LookAt = default;
            TargetForward = default;
           
            ObjectPool.Instance.Recycle(this);
        }

        public void Lerp(CameraStateData from, CameraStateData to, float lerpVal)
        {
            lerpVal = Mathf.Clamp01(lerpVal);
            Fov = Mathf.Lerp(from.Fov, to.Fov, lerpVal);
            // NearClipPlane = default;
            // Up = default;
            // Forward = default;
            // Position = default;
            // Orientation = default;
            // LookAt = default;
            // TargetForward = default;
        }
    }
}