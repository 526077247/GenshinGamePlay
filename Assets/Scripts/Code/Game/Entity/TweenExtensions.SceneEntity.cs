using UnityEngine;

namespace TaoTie
{
    public static partial class TweenExtensions
    {
        #region SceneEntity — Position

        public static Tweener<Vector3> DOMove(this Unit target, Vector3 endValue, int duration)
        {
            return TweenManager.Instance.ToVector3(target,
                () => target.Position, v => target.Position = v, endValue, duration);
        }

        #endregion

        #region SceneEntity — Rotation (Euler)

        public static Tweener<Vector3> DORotate(this Unit target, Vector3 endValue, int duration)
        {
            return TweenManager.Instance.ToVector3(target,
                () => target.Rotation.eulerAngles, v => target.Rotation = Quaternion.Euler(v), endValue, duration);
        }

        public static Tweener<Quaternion> DORotateQuaternion(this Unit target, Quaternion endValue, int duration)
        {
            return TweenManager.Instance.ToQuaternion(target,
                () => target.Rotation, v => target.Rotation = v, endValue, duration);
        }

        public static Tweener<Vector3> DOForward(this Unit target, Vector3 endValue, int duration)
        {
            return TweenManager.Instance.ToVector3(target,
                () => target.Forward, v => target.Forward = v, endValue, duration);
        }

        #endregion

        #region SceneEntity — Scale

        public static Tweener<Vector3> DOScale(this Unit target, Vector3 endValue, int duration)
        {
            return TweenManager.Instance.ToVector3(target,
                () => target.LocalScale, v => target.LocalScale = v, endValue, duration);
        }

        public static Tweener<Vector3> DOScale(this Unit target, float endValue, int duration)
        {
            return TweenManager.Instance.ToVector3(target,
                () => target.LocalScale, v => target.LocalScale = v, Vector3.one * endValue, duration);
        }

        #endregion
    }
}
