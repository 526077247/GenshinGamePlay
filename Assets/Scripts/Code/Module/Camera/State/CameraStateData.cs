using System;
using UnityEngine;
namespace TaoTie
{
    public class CameraStateData: IDisposable
    {
        /// <summary>正交/透视互换时，投影形式默认在过渡进度的这个点上切换</summary>
        public const float DefaultProjectCrossPoint = 0.5f;

        private const float ProjectCrossPointMin = 0.01f;
        private const float ProjectCrossPointMax = 0.99f;
        private const float MinFov = 1f;
        private const float MaxFov = 179f;
        private const float MinFocusDist = 0.01f;

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
            // 注意：ObjectPool.Fetch 不会清空复用对象，字段必须逐一赋值，漏拷会带上一帧的脏数据
            var res = ObjectPool.Instance.Fetch<CameraStateData>();
            res.Fov = other.Fov;
            res.OrthographicnSize = other.OrthographicnSize;
            res.Orthographicn = other.Orthographicn;
            res.NearClipPlane = other.NearClipPlane;
            res.FarClipPlane = other.FarClipPlane;
            res.Up = other.Up;
            res.Forward = other.Forward;
            res.SphereQuaternion = other.SphereQuaternion;
            res.Position = other.Position;
            res.Orientation = other.Orientation;
            res.LookAt = other.LookAt;
            res.TargetForward = other.TargetForward;
            res.TargetUp = other.TargetUp;
            res.AvatarFaceDirection = other.AvatarFaceDirection;
            return res;
        }
        public void Dispose()
        {
            Fov = default;
            OrthographicnSize = default;
            Orthographicn = default;
            NearClipPlane = default;
            FarClipPlane = default;
            Up = default;
            Forward = default;
            SphereQuaternion = default;
            Position = default;
            Orientation = default;
            LookAt = default;
            TargetForward = default;
            TargetUp = default;
            AvatarFaceDirection = false;
            ObjectPool.Instance.Recycle(this);
        }

        /// <summary>
        /// 相机状态数据插值。
        /// </summary>
        /// <param name="from">起始状态（引用，会被持续更新）</param>
        /// <param name="to">目标状态（引用，会被持续更新）</param>
        /// <param name="lerpVal">过渡进度 [0,1]</param>
        /// <param name="projectCrossPoint">正交/透视互换时，投影形式的切换进度点</param>
        /// <param name="transitionDist">过渡期焦点距离覆盖，&lt;=0 时用相机到注视点的实时距离</param>
        /// <param name="smoothProjectSwap">是否在切换点自动降速</param>
        public void Lerp(CameraStateData from, CameraStateData to, float lerpVal,
            float projectCrossPoint = DefaultProjectCrossPoint,
            float transitionDist = 0f, bool smoothProjectSwap = true)
        {
            lerpVal = Mathf.Clamp01(lerpVal);
            bool projectDiff = from.Orthographicn != to.Orthographicn;
            float cross = Mathf.Clamp(projectCrossPoint, ProjectCrossPointMin, ProjectCrossPointMax);

            // 正交/透视互换时，把进度重映射成"在切换点速度归零"的两段曲线：
            // 投影矩阵只能硬切，但可以让切换发生在画面几乎不动的一刻，形变就藏住了。
            if (projectDiff && smoothProjectSwap)
            {
                lerpVal = SmoothTwoPhase(lerpVal, cross);
            }

            // 位置与朝向先行，焦点距离由它们导出
            Position = Vector3.Lerp(from.Position, to.Position, lerpVal);
            if (Quaternion.Dot(from.Orientation, to.Orientation) < Quaternion.kEpsilon)
            {
                Orientation = from.Orientation;
            }
            else
            {
                Orientation = Quaternion.Lerp(from.Orientation, to.Orientation, lerpVal);
            }

            LookAt = Vector3.Lerp(from.LookAt, to.LookAt, lerpVal);
            TargetForward = Vector3.Lerp(from.TargetForward, to.TargetForward, lerpVal);
            TargetUp = Vector3.Lerp(from.TargetUp, to.TargetUp, lerpVal);
            if (Quaternion.Dot(from.SphereQuaternion, to.SphereQuaternion) < Quaternion.kEpsilon)
            {
                SphereQuaternion = from.SphereQuaternion;
            }
            else
            {
                SphereQuaternion = Quaternion.Slerp(from.SphereQuaternion, to.SphereQuaternion, lerpVal);
            }

            if (projectDiff)
            {
                // 透视的可视半高 = 焦点距离 * tan(fov/2)，正交的可视半高 = orthographicSize。
                // 两者只有用同一个焦点距离换算，投影切换前后的取景才是连续的；
                // 用固定参考距离会让远处机位（大地图 ~190）算出的半高与真实画面对不上，
                // 翻转那一帧就会出现一次肉眼可见的骤然缩放。
                var dist = transitionDist > MinFocusDist
                    ? transitionDist
                    : Vector3.Distance(Position, LookAt);
                dist = Mathf.Max(MinFocusDist, dist);

                var half = Mathf.Lerp(HalfHeightAt(from, dist), HalfHeightAt(to, dist), lerpVal);
                // 阶段一保持 from 的投影形式，阶段二切到 to 的投影形式。
                // 切换点两侧的焦平面可视半高都是 half，因此不会跳变。
                Orthographicn = lerpVal <= cross ? from.Orthographicn : to.Orthographicn;
                if (Orthographicn)
                {
                    OrthographicnSize = half;
                }
                else
                {
                    Fov = Mathf.Clamp(2f * Mathf.Atan(half / dist) * Mathf.Rad2Deg, MinFov, MaxFov);
                }
            }
            else
            {
                // 非投影切换：沿用直接插值语义，不改变既有观感
                Orthographicn = from.Orthographicn;
                Fov = Mathf.Lerp(from.Fov, to.Fov, lerpVal);
                OrthographicnSize = Mathf.Lerp(from.OrthographicnSize, to.OrthographicnSize, lerpVal);
            }

            NearClipPlane = Mathf.Lerp(from.NearClipPlane, to.NearClipPlane, lerpVal);
            FarClipPlane = Mathf.Lerp(from.FarClipPlane, to.FarClipPlane, lerpVal);

            AvatarFaceDirection = to.AvatarFaceDirection;
        }

        /// <summary>给定焦点距离下，该状态数据在焦点平面上的可视半高。</summary>
        private static float HalfHeightAt(CameraStateData d, float dist)
        {
            return d.Orthographicn
                ? d.OrthographicnSize
                : dist * Mathf.Tan(d.Fov * 0.5f * Mathf.Deg2Rad);
        }

        /// <summary>
        /// 两段重映射：把 [0,1] 的进度重映射成在 cross 处速度归零的曲线。
        /// 端点与切换点保持不动（0 -&gt; 0，cross -&gt; cross，1 -&gt; 1），因此不影响首尾帧的精确衔接。
        /// </summary>
        private static float SmoothTwoPhase(float t, float cross)
        {
            if (t <= 0f) return 0f;
            if (t >= 1f) return 1f;
            return t < cross
                ? Mathf.SmoothStep(0f, cross, t / cross)
                : Mathf.SmoothStep(cross, 1f, (t - cross) / (1f - cross));
        }
    }
}
