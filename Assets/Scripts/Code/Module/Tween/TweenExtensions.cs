using System;
using UnityEngine;
using UnityEngine.UI;

namespace TaoTie
{
    public static partial class TweenExtensions
    {
        #region Transform

        public static Tweener<Vector3> DOMove(this Transform target, Vector3 endValue, int duration)
        {
            return TweenManager.Instance.ToVector3(target,
                () => target.position, v => target.position = v, endValue, duration);
        }

        public static Tweener<Vector3> DOLocalMove(this Transform target, Vector3 endValue, int duration)
        {
            return TweenManager.Instance.ToVector3(target,
                () => target.localPosition, v => target.localPosition = v, endValue, duration);
        }

        public static Tweener<Vector3> DOScale(this Transform target, Vector3 endValue, int duration)
        {
            return TweenManager.Instance.ToVector3(target,
                () => target.localScale, v => target.localScale = v, endValue, duration);
        }

        public static Tweener<Vector3> DOScale(this Transform target, float endValue, int duration)
        {
            return TweenManager.Instance.ToVector3(target,
                () => target.localScale, v => target.localScale = v, Vector3.one * endValue, duration);
        }

        public static Tweener<Vector3> DORotate(this Transform target, Vector3 endValue, int duration)
        {
            return TweenManager.Instance.ToVector3(target,
                () => target.eulerAngles, v => target.eulerAngles = v, endValue, duration);
        }

        public static Tweener<Vector3> DOLocalRotate(this Transform target, Vector3 endValue, int duration)
        {
            return TweenManager.Instance.ToVector3(target,
                () => target.localEulerAngles, v => target.localEulerAngles = v, endValue, duration);
        }

        #endregion

        #region RectTransform

        public static Tweener<Vector2> DOAnchorPos(this RectTransform target, Vector2 endValue, int duration)
        {
            return TweenManager.Instance.ToVector2(target,
                () => target.anchoredPosition, v => target.anchoredPosition = v, endValue, duration);
        }

        public static Tweener<Vector2> DOSizeDelta(this RectTransform target, Vector2 endValue, int duration)
        {
            return TweenManager.Instance.ToVector2(target,
                () => target.sizeDelta, v => target.sizeDelta = v, endValue, duration);
        }

        public static Tweener<Vector3> DOAnchorPos3D(this RectTransform target, Vector3 endValue, int duration)
        {
            return TweenManager.Instance.ToVector3(target,
                () => target.anchoredPosition3D, v => target.anchoredPosition3D = v, endValue, duration);
        }

        public static Tweener<Vector2> DOAnchorMax(this RectTransform target, Vector2 endValue, int duration)
        {
            return TweenManager.Instance.ToVector2(target,
                () => target.anchorMax, v => target.anchorMax = v, endValue, duration);
        }

        public static Tweener<Vector2> DOAnchorMin(this RectTransform target, Vector2 endValue, int duration)
        {
            return TweenManager.Instance.ToVector2(target,
                () => target.anchorMin, v => target.anchorMin = v, endValue, duration);
        }

        public static Tweener<Vector2> DOPivot(this RectTransform target, Vector2 endValue, int duration)
        {
            return TweenManager.Instance.ToVector2(target,
                () => target.pivot, v => target.pivot = v, endValue, duration);
        }

        #endregion

        #region Material

        public static Tweener<Color> DOColor(this Material target, Color endValue, int duration)
        {
            return TweenManager.Instance.ToColor(target,
                () => target.color, v => target.color = v, endValue, duration);
        }

        public static Tweener<float> DOFloat(this Material target, string property, float endValue, int duration)
        {
            return TweenManager.Instance.ToFloat(target,
                () => target.GetFloat(property), v => target.SetFloat(property, v), endValue, duration);
        }

        public static Tweener<Color> DOColor(this Material target, string property, Color endValue, int duration)
        {
            return TweenManager.Instance.ToColor(target,
                () => target.GetColor(property), v => target.SetColor(property, v), endValue, duration);
        }

        #endregion

        #region CanvasGroup

        public static Tweener<float> DOFade(this CanvasGroup target, float endValue, int duration)
        {
            return TweenManager.Instance.ToFloat(target,
                () => target.alpha, v => target.alpha = v, endValue, duration);
        }

        #endregion

        #region SpriteRenderer

        public static Tweener<Color> DOColor(this SpriteRenderer target, Color endValue, int duration)
        {
            return TweenManager.Instance.ToColor(target,
                () => target.color, v => target.color = v, endValue, duration);
        }

        #endregion

        #region Image

        public static Tweener<Color> DOColor(this Image target, Color endValue, int duration)
        {
            return TweenManager.Instance.ToColor(target,
                () => target.color, v => target.color = v, endValue, duration);
        }

        public static Tweener<float> DOFillAmount(this Image target, float endValue, int duration)
        {
            return TweenManager.Instance.ToFloat(target,
                () => target.fillAmount, v => target.fillAmount = v, endValue, duration);
        }

        #endregion

        #region Text / TextMeshPro

        public static Tweener<Color> DOColor(this Text target, Color endValue, int duration)
        {
            return TweenManager.Instance.ToColor(target,
                () => target.color, v => target.color = v, endValue, duration);
        }

        #endregion

        #region Generic

        public static Tweener<float> DOValue(this object target, Func<float> getter, Action<float> setter,
            float endValue, int duration)
        {
            return TweenManager.Instance.ToFloat(target, getter, setter, endValue, duration);
        }

        #endregion
    }
}
