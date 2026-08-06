using System;
using System.Collections.Generic;

namespace TaoTie
{
    public class Sequence : Tween
    {
        private struct SequenceItem
        {
            public Tween tween;
            public int duration;
            public Action callback;
            public bool isCallback;
        }

        private static readonly Queue<Sequence> pool = new Queue<Sequence>();

        private readonly List<SequenceItem> items = new List<SequenceItem>();
        private int currentIndex;
        private int currentElapsed;
        private bool currentStarted;

        internal static Sequence Create(TweenManager manager)
        {
            var seq = pool.Count > 0 ? pool.Dequeue() : new Sequence();
            seq.InitInternal(manager);
            return seq;
        }

        private void InitInternal(TweenManager mgr)
        {
            manager = mgr;
            items.Clear();
            currentIndex = 0;
            currentElapsed = 0;
            currentStarted = false;
            duration = 0;
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

        public Sequence Append(Tween tween)
        {
            tween.managedExternally = true;
            tween.IsPlaying = false;
            items.Add(new SequenceItem { tween = tween, duration = tween.Duration });
            duration += tween.Duration;
            return this;
        }

        public Sequence AppendInterval(int interval)
        {
            items.Add(new SequenceItem { duration = interval });
            duration += interval;
            return this;
        }

        public Sequence AppendCallback(Action callback)
        {
            items.Add(new SequenceItem { callback = callback, isCallback = true });
            return this;
        }

        internal override void DoUpdate(int deltaMs)
        {
            if (IsKilled || IsComplete || IsPaused || !IsPlaying) return;

            if (cancellationToken != null && cancellationToken.IsDispose())
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

            if (currentIndex >= items.Count)
            {
                Finish();
                return;
            }

            elapsed += deltaMs;
            currentElapsed += deltaMs;

            var item = items[currentIndex];

            if (!currentStarted)
            {
                currentStarted = true;

                if (item.isCallback)
                {
                    try
                    {
                        item.callback?.Invoke();
                    }
                    catch (Exception e)
                    {
                        Log.Error(e);
                    }

                    currentIndex++;
                    currentElapsed = 0;
                    currentStarted = false;
                }
                else if (item.tween != null)
                {
                    item.tween.IsPlaying = true;
                    item.tween.DoUpdate(0);
                }
            }

            if (item.tween != null)
            {
                item.tween.DoUpdate(deltaMs);

                if (item.tween.IsComplete || item.tween.IsKilled)
                {
                    currentIndex++;
                    currentElapsed = 0;
                    currentStarted = false;
                }
            }
            else if (!item.isCallback)
            {
                if (currentElapsed >= item.duration)
                {
                    currentIndex++;
                    currentElapsed = 0;
                    currentStarted = false;
                }
            }

            onUpdate?.Invoke();

            if (currentIndex >= items.Count)
            {
                Finish();
            }
        }

        protected override void UpdateValue(float normalizedTime) { }

        public override void Complete()
        {
            if (IsKilled || IsComplete) return;

            delayElapsed = delay;

            if (!started)
            {
                started = true;
                OnTweenStart();
                onStart?.Invoke();
            }

            for (int i = currentIndex; i < items.Count; i++)
            {
                var item = items[i];
                if (item.isCallback)
                {
                    try { item.callback?.Invoke(); }
                    catch (Exception e) { Log.Error(e); }
                }
                else if (item.tween != null)
                {
                    item.tween.IsPlaying = true;
                    item.tween.Complete();
                }
                currentIndex = i + 1;
            }

            Finish();
        }

        public override void Kill(bool complete = false)
        {
            if (IsKilled) return;

            if (complete && !IsComplete)
            {
                Complete();
            }

            IsKilled = true;

            for (int i = currentIndex; i < items.Count; i++)
            {
                var item = items[i];
                if (item.tween != null && !item.tween.IsKilled)
                {
                    item.tween.Kill(false);
                }
            }

            if (!IsComplete)
            {
                IsComplete = true;
            }

            onKill?.Invoke();
            Cleanup();
        }

        internal override void Reset()
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].tween != null)
                {
                    if (!items[i].tween.IsKilled)
                    {
                        items[i].tween.Kill(false);
                    }
                    items[i].tween.Reset();
                }
            }
            items.Clear();
            currentIndex = 0;
            currentElapsed = 0;
            currentStarted = false;

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
            manager = null;

            if (pool.Count < 500)
            {
                pool.Enqueue(this);
            }
        }
    }
}
