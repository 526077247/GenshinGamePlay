using System;
using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    public class Tweener<T> : Tween
    {
        private static readonly Queue<Tweener<T>> pool = new Queue<Tweener<T>>();

        private Func<T> getter;
        private Action<T> setter;
        private T startValue;
        private T endValue;
        private Func<T, T, float, T> lerpFunc;

        internal static Tweener<T> Create(
            object target,
            Func<T> getter,
            Action<T> setter,
            T endValue,
            int duration,
            Func<T, T, float, T> lerpFunc,
            TweenManager manager)
        {
            var tweener = pool.Count > 0 ? pool.Dequeue() : new Tweener<T>();
            tweener.InitInternal(target, getter, setter, endValue, duration, lerpFunc, manager);
            return tweener;
        }

        private void InitInternal(
            object target,
            Func<T> getter,
            Action<T> setter,
            T endValue,
            int duration,
            Func<T, T, float, T> lerpFunc,
            TweenManager manager)
        {
            Target = target;
            this.getter = getter;
            this.setter = setter;
            this.endValue = endValue;
            this.duration = duration;
            this.lerpFunc = lerpFunc;
            this.manager = manager;

            delay = 0;
            delayElapsed = 0;
            elapsed = 0;
            ease = EasingFunction.Ease.Linear;
            easeFunc = EasingFunction.GetEasingFunction(EasingFunction.Ease.Linear);
            loops = 1;
            loopType = LoopType.Restart;
            loopCount = 0;
            isReversed = false;
            useUnscaledTime = false;
            started = false;
            IsPlaying = true;
            IsPaused = false;
            IsKilled = false;
            IsComplete = false;
            taskCreated = false;
            completionTask = null;
            cancellationToken = null;
            cancelAction = null;
            onStart = null;
            onUpdate = null;
            onComplete = null;
            onKill = null;
            managedExternally = false;
        }

        protected override void OnTweenStart()
        {
            startValue = getter();
        }

        protected override void UpdateValue(float normalizedTime)
        {
            setter(lerpFunc(startValue, endValue, normalizedTime));
        }

        internal override void Reset()
        {
            Target = null;
            getter = null;
            setter = null;
            startValue = default;
            endValue = default;
            lerpFunc = null;
            manager = null;

            delay = 0;
            delayElapsed = 0;
            elapsed = 0;
            duration = 0;
            ease = EasingFunction.Ease.Linear;
            easeFunc = null;
            loops = 1;
            loopType = LoopType.Restart;
            loopCount = 0;
            isReversed = false;
            useUnscaledTime = false;
            started = false;
            IsPlaying = false;
            IsPaused = false;
            IsKilled = false;
            IsComplete = false;
            taskCreated = false;
            completionTask = null;
            cancellationToken = null;
            cancelAction = null;
            onStart = null;
            onUpdate = null;
            onComplete = null;
            onKill = null;
            managedExternally = false;

            if (pool.Count < 1000)
            {
                pool.Enqueue(this);
            }
        }
    }

    public static class TweenLerp
    {
        public static float LerpFloat(float a, float b, float t)
        {
            return a + (b - a) * t;
        }

        public static Vector3 LerpVector3(Vector3 a, Vector3 b, float t)
        {
            return Vector3.LerpUnclamped(a, b, t);
        }

        public static Vector2 LerpVector2(Vector2 a, Vector2 b, float t)
        {
            return Vector2.LerpUnclamped(a, b, t);
        }

        public static Color LerpColor(Color a, Color b, float t)
        {
            return Color.LerpUnclamped(a, b, t);
        }

        public static Quaternion LerpQuaternion(Quaternion a, Quaternion b, float t)
        {
            return Quaternion.SlerpUnclamped(a, b, t);
        }
    }
}
