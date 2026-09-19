using System;
using UnityEngine;
namespace TaoTie
{
    public class CameraStateData: IDisposable
    {        
        /// <summary>
        /// 正交/透视互换时，把 Fov 与 OrthographicSize 换算成“可视半高”所用的固定参考距离
        /// </summary>
        private const float OrthoReferenceDist = 10f;
        
        public float Fov;
        public float OrthographicnSize;
        public float NearClipPlane;
        public float FarClipPlane;
        public bool Orthographicn;
        
        public Vector3 Up;
        public Vector3 Forward;
        /// <summary>
        /// 相机相对目标的旋转
        /// </summary>
        public Quaternion SphereQuaternion;
        
        public Vector3 Position;
        public Quaternion Orientation;
        
        public Vector3 LookAt;
        public Vector3 TargetForward;
        public Vector3 TargetUp;

        public bool AvatarFaceDirection;
        public static CameraStateData Create()
        {
            return ObjectPool.Instance.Fetch<CameraStateData>();
        }
        
        public static CameraStateData DeepClone(CameraStateData other)
        {
            var res = ObjectPool.Instance.Fetch<CameraStateData>();
            res.Fov = other.Fov;
            res.OrthographicnSize = other.OrthographicnSize;
            res.Orthographicn = other.Orthographicn;
            res.NearClipPlane = other.NearClipPlane;
            res.FarClipPlane = other.FarClipPlane;
            res.Up = other.Up;
            res.Forward = other.Forward;
            res.Position = other.Position;
            res.Orientation = other.Orientation;
            res.LookAt = other.LookAt;
            res.TargetForward = other.TargetForward;
            res.AvatarFaceDirection = other.AvatarFaceDirection;
            return res;
        }
        public void Dispose()
        {
            Fov = default;
            Orthographicn = default;
            NearClipPlane = default;
            FarClipPlane = default;
            Up = default;
            Forward = default;
            Position = default;
            Orientation = default;
            LookAt = default;
            TargetForward = default;
            AvatarFaceDirection = false;
            ObjectPool.Instance.Recycle(this);
        }

        public void Lerp(CameraStateData from, CameraStateData to, float lerpVal)
        {
            lerpVal = Mathf.Clamp01(lerpVal);
            if (to.Orthographicn != from.Orthographicn)
            {
                var fromWidth = from.Orthographicn
                    ? from.OrthographicnSize
                    : OrthoReferenceDist * Mathf.Tan(from.Fov * 0.5f * Mathf.Deg2Rad);
                var toWidth = to.Orthographicn
                    ? to.OrthographicnSize
                    : OrthoReferenceDist * Mathf.Tan(to.Fov * 0.5f * Mathf.Deg2Rad);
                var width = Mathf.Lerp(fromWidth, toWidth, lerpVal);
                Orthographicn = lerpVal > 0.5f ? to.Orthographicn : from.Orthographicn;
                OrthographicnSize = width;
                Fov = Mathf.Clamp(2f * Mathf.Atan(width / OrthoReferenceDist) * Mathf.Rad2Deg, 1f, 179f);
            }
            else
            {
                Orthographicn = from.Orthographicn;
                Fov = Mathf.Lerp(from.Fov, to.Fov, lerpVal);
                OrthographicnSize = Mathf.Lerp(from.OrthographicnSize, to.OrthographicnSize, lerpVal);
            }
            NearClipPlane = Mathf.Lerp(from.NearClipPlane, to.NearClipPlane, lerpVal);
            FarClipPlane = Mathf.Lerp(from.FarClipPlane, to.FarClipPlane, lerpVal);
            Position = Vector3.Lerp(from.Position, to.Position, lerpVal);
            if (Quaternion.Dot(from.Orientation, to.Orientation) < Quaternion.kEpsilon)
            {
                Orientation = from.Orientation;
            }
            else
            {
                Orientation = Quaternion.Lerp(from.Orientation, to.Orientation, lerpVal);
            }

            AvatarFaceDirection = to.AvatarFaceDirection;
        }
    }
}