using System;
using UnityEngine;

namespace TaoTie
{
    public enum LoopType
    {
        Restart,
        Yoyo,
        Incremental
    }

    public abstract class Tween
    {
        public object Target { get; protected set; }
        public bool IsPlaying { get; internal set; }
        public bool IsPaused { get; protected set; }
        public bool IsKilled { get; protected set; }
        public bool IsComplete { get; protected set; }

        protected int delay;
        protected int delayElapsed;
        protected int duration;
        protected int elapsed;
        protected bool useUnscaledTime;
        internal bool UseUnscaledTime => useUnscaledTime;

        protected EasingFunction.Ease ease = EasingFunction.Ease.Linear;
        protected EasingFunction.Function easeFunc;

        protected int loops = 1;
        protected LoopType loopType = LoopType.Restart;
        protected int loopCount;
        protected bool isReversed;

        protected Action onStart;
        protected Action onUpdate;
        protected Action onComplete;
        protected Action onKill;
        protected bool started;

        protected ETTask completionTask;
        protected bool taskCreated;

        protected ETCancellationToken cancellationToken;
        protected Action cancelAction;

        protected TweenManager manager;

        internal bool managedExternally;

        public int Duration => duration;

        #region Fluent API

        public Tween SetEase(EasingFunction.Ease ease)
        {
            this.ease = ease;
            this.easeFunc = EasingFunction.GetEasingFunction(ease);
            return this;
        }

        public Tween SetDelay(int delay)
        {
            this.delay = delay;
            return this;
        }

        public Tween SetLoops(int loops, LoopType loopType = LoopType.Restart)
        {
            this.loops = loops;
            this.loopType = loopType;
            return this;
        }

        public Tween SetLink(ETCancellationToken cancellationToken)
        {
            this.cancellationToken = cancellationToken;
            this.cancelAction = () => Kill(false);
            cancellationToken.Add(cancelAction);
            return this;
        }

        public Tween OnStart(Action callback)
        {
            onStart = callback;
            return this;
        }

        public Tween OnUpdate(Action callback)
        {
            onUpdate = callback;
            return this;
        }

        public Tween OnComplete(Action callback)
        {
            onComplete = callback;
            return this;
        }

        public Tween OnKill(Action callback)
        {
            onKill = callback;
            return this;
        }

        public Tween SetUnscaledTime(bool unscaled)
        {
            useUnscaledTime = unscaled;
            return this;
        }

        #endregion

        #region Awaitable

        public ETTask GetAwaiter()
        {
            if (!taskCreated)
            {
                completionTask = ETTask.Create(true);
                taskCreated = true;
            }
            return completionTask;
        }

        #endregion

        #region Control

        public void Play()
        {
            IsPaused = false;
        }

        public void Pause()
        {
            IsPaused = true;
        }

        public virtual void Complete()
        {
            if (IsKilled || IsComplete) return;

            elapsed = duration;
            delayElapsed = delay;

            if (!started)
            {
                started = true;
                OnTweenStart();
                onStart?.Invoke();
            }

            float t = GetEasedTime(1f);
            UpdateValue(t);
            onUpdate?.Invoke();

            Finish();
        }

        public virtual void Kill(bool complete = false)
        {
            if (IsKilled) return;

            if (complete && !IsComplete)
            {
                Complete();
            }

            IsKilled = true;

            if (!IsComplete)
            {
                IsComplete = true;
            }

            onKill?.Invoke();
            Cleanup();
        }

        #endregion

        #region Internal

        internal virtual void DoUpdate(int deltaMs)
        {
            if (IsKilled || IsComplete || IsPaused || !IsPlaying) return;

            if (cancellationToken != null && cancellationToken.IsDispose())
            {
                Kill(false);
                return;
            }

            if (Target is UnityEngine.Object unityObj && unityObj == null)
            {
                Kill(false);
                return;
            }

            if (!started)
            {
                delayElapsed += deltaMs;
                if (delayElapsed < delay) return;

                started = true;
                OnTweenStart();
                onStart?.Invoke();
            }

            elapsed += deltaMs;

            int clampedElapsed = elapsed;
            if (clampedElapsed > duration) clampedElapsed = duration;

            float normalizedTime = duration > 0 ? (float)clampedElapsed / duration : 1f;

            if (isReversed && loopType == LoopType.Yoyo)
            {
                normalizedTime = 1f - normalizedTime;
            }

            float easedTime = easeFunc != null
                ? easeFunc(normalizedTime, 0f, 1f, 1f)
                : normalizedTime;

            UpdateValue(easedTime);
            onUpdate?.Invoke();

            if (elapsed >= duration)
            {
                if (loops < 0 || loopCount < loops - 1)
                {
                    loopCount++;
                    elapsed -= duration;

                    if (loopType == LoopType.Yoyo)
                    {
                        isReversed = !isReversed;
                    }
                }
                else
                {
                    Finish();
                }
            }
        }

        protected float GetEasedTime(float normalizedTime)
        {
            if (isReversed && loopType == LoopType.Yoyo)
            {
                normalizedTime = 1f - normalizedTime;
            }

            return easeFunc != null
                ? easeFunc(normalizedTime, 0f, 1f, 1f)
                : normalizedTime;
        }

        protected virtual void Finish()
        {
            if (IsComplete) return;
            IsComplete = true;

            try
            {
                onComplete?.Invoke();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }

            if (taskCreated && !completionTask.IsCompleted)
            {
                ETTask task = completionTask;
                completionTask = null;
                taskCreated = false;
                task.SetResult();
            }
        }

        protected virtual void Cleanup()
        {
            if (cancellationToken != null && cancelAction != null)
            {
                cancellationToken.Remove(cancelAction);
                cancellationToken = null;
                cancelAction = null;
            }

            onStart = null;
            onUpdate = null;
            onComplete = null;
            onKill = null;

            if (taskCreated && !completionTask.IsCompleted)
            {
                ETTask task = completionTask;
                completionTask = null;
                taskCreated = false;
                task.SetResult();
            }
        }

        protected abstract void UpdateValue(float normalizedTime);

        protected virtual void OnTweenStart() { }

        internal abstract void Reset();

        #endregion
    }
}
