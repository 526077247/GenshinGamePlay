using UnityEngine;

namespace TaoTie
{
    public static partial class TweenExtensions
    {
        #region UIBaseContainer — Transform

        public static Tweener<Vector3> DOMove(this UIBaseContainer target, Vector3 endValue, int duration)
        {
            var transform = target.GetTransform();
            return TweenManager.Instance.ToVector3(target,
                () => transform.position, v => transform.position = v, endValue, duration);
        }

        public static Tweener<Vector3> DOLocalMove(this UIBaseContainer target, Vector3 endValue, int duration)
        {
            var transform = target.GetTransform();
            return TweenManager.Instance.ToVector3(target,
                () => transform.localPosition, v => transform.localPosition = v, endValue, duration);
        }

        public static Tweener<Vector3> DOScale(this UIBaseContainer target, Vector3 endValue, int duration)
        {
            var transform = target.GetTransform();
            return TweenManager.Instance.ToVector3(target,
                () => transform.localScale, v => transform.localScale = v, endValue, duration);
        }

        public static Tweener<Vector3> DOScale(this UIBaseContainer target, float endValue, int duration)
        {
            var transform = target.GetTransform();
            return TweenManager.Instance.ToVector3(target,
                () => transform.localScale, v => transform.localScale = v, Vector3.one * endValue, duration);
        }

        public static Tweener<Vector3> DORotate(this UIBaseContainer target, Vector3 endValue, int duration)
        {
            var transform = target.GetTransform();
            return TweenManager.Instance.ToVector3(target,
                () => transform.eulerAngles, v => transform.eulerAngles = v, endValue, duration);
        }

        public static Tweener<Vector3> DOLocalRotate(this UIBaseContainer target, Vector3 endValue, int duration)
        {
            var transform = target.GetTransform();
            return TweenManager.Instance.ToVector3(target,
                () => transform.localEulerAngles, v => transform.localEulerAngles = v, endValue, duration);
        }

        #endregion

        #region UIBaseContainer — RectTransform

        public static Tweener<Vector2> DOAnchorPos(this UIBaseContainer target, Vector2 endValue, int duration)
        {
            var rectTransform = target.GetRectTransform();
            return TweenManager.Instance.ToVector2(target,
                () => rectTransform.anchoredPosition, v => rectTransform.anchoredPosition = v, endValue, duration);
        }

        public static Tweener<Vector2> DOSizeDelta(this UIBaseContainer target, Vector2 endValue, int duration)
        {
            var rectTransform = target.GetRectTransform();
            return TweenManager.Instance.ToVector2(target,
                () => rectTransform.sizeDelta, v => rectTransform.sizeDelta = v, endValue, duration);
        }

        public static Tweener<Vector3> DOAnchorPos3D(this UIBaseContainer target, Vector3 endValue, int duration)
        {
            var rectTransform = target.GetRectTransform();
            return TweenManager.Instance.ToVector3(target,
                () => rectTransform.anchoredPosition3D, v => rectTransform.anchoredPosition3D = v, endValue, duration);
        }

        public static Tweener<Vector2> DOAnchorMax(this UIBaseContainer target, Vector2 endValue, int duration)
        {
            var rectTransform = target.GetRectTransform();
            return TweenManager.Instance.ToVector2(target,
                () => rectTransform.anchorMax, v => rectTransform.anchorMax = v, endValue, duration);
        }

        public static Tweener<Vector2> DOAnchorMin(this UIBaseContainer target, Vector2 endValue, int duration)
        {
            var rectTransform = target.GetRectTransform();
            return TweenManager.Instance.ToVector2(target,
                () => rectTransform.anchorMin, v => rectTransform.anchorMin = v, endValue, duration);
        }

        public static Tweener<Vector2> DOPivot(this UIBaseContainer target, Vector2 endValue, int duration)
        {
            var rectTransform = target.GetRectTransform();
            return TweenManager.Instance.ToVector2(target,
                () => rectTransform.pivot, v => rectTransform.pivot = v, endValue, duration);
        }

        #endregion

        #region UIBaseContainer — GameObject

        public static Tweener<float> DOFade(this UIBaseContainer target, float endValue, int duration)
        {
            var canvasGroup = target.GetGameObject().GetComponent<CanvasGroup>();
            return TweenManager.Instance.ToFloat(target,
                () => canvasGroup.alpha, v => canvasGroup.alpha = v, endValue, duration);
        }

        #endregion
    }
}
