using System;
using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    public class TweenManager : IManager, IUpdate
    {
        public static TweenManager Instance { get; private set; }

        private readonly LinkedList<Tween> activeTweens = new LinkedList<Tween>();
        private readonly Dictionary<object, List<Tween>> targetTweens = new Dictionary<object, List<Tween>>();
        private readonly Queue<Tween> pendingAdd = new Queue<Tween>();

        public void Init()
        {
            Instance = this;
        }

        public void Destroy()
        {
            Instance = null;
            foreach (var tween in activeTweens)
            {
                tween.Kill(false);
                tween.Reset();
            }
            activeTweens.Clear();
            targetTweens.Clear();
            pendingAdd.Clear();
        }

        public void Update()
        {
            while (pendingAdd.Count > 0)
            {
                var tween = pendingAdd.Dequeue();
                if (tween.managedExternally) continue;
                activeTweens.AddLast(tween);
                if (tween.Target != null)
                {
                    if (!targetTweens.TryGetValue(tween.Target, out var list))
                    {
                        list = new List<Tween>();
                        targetTweens[tween.Target] = list;
                    }
                    list.Add(tween);
                }
            }

            int dt = (int)(Time.deltaTime * 1000f);
            int unscaledDt = (int)(Time.unscaledDeltaTime * 1000f);

            var node = activeTweens.First;
            while (node != null)
            {
                var next = node.Next;
                var tween = node.Value;

                if (tween.IsKilled)
                {
                    activeTweens.Remove(node);
                    RemoveFromTargetMap(tween);
                    tween.Reset();
                }
                else
                {
                    if (!tween.IsPaused)
                    {
                        int delta = tween.UseUnscaledTime ? unscaledDt : dt;
                        tween.DoUpdate(delta);
                    }

                    if (tween.IsKilled || tween.IsComplete)
                    {
                        activeTweens.Remove(node);
                        RemoveFromTargetMap(tween);
                        tween.Reset();
                    }
                }

                node = next;
            }
        }

        private void RemoveFromTargetMap(Tween tween)
        {
            if (tween.Target == null) return;
            if (targetTweens.TryGetValue(tween.Target, out var list))
            {
                list.Remove(tween);
                if (list.Count == 0)
                {
                    targetTweens.Remove(tween.Target);
                }
            }
        }

        #region Create Tweens

        public Tweener<float> ToFloat(object target, Func<float> getter, Action<float> setter, float endValue, int duration)
        {
            var tweener = Tweener<float>.Create(target, getter, setter, endValue, duration, TweenLerp.LerpFloat, this);
            pendingAdd.Enqueue(tweener);
            return tweener;
        }

        public Tweener<Vector3> ToVector3(object target, Func<Vector3> getter, Action<Vector3> setter, Vector3 endValue, int duration)
        {
            var tweener = Tweener<Vector3>.Create(target, getter, setter, endValue, duration, TweenLerp.LerpVector3, this);
            pendingAdd.Enqueue(tweener);
            return tweener;
        }

        public Tweener<Vector2> ToVector2(object target, Func<Vector2> getter, Action<Vector2> setter, Vector2 endValue, int duration)
        {
            var tweener = Tweener<Vector2>.Create(target, getter, setter, endValue, duration, TweenLerp.LerpVector2, this);
            pendingAdd.Enqueue(tweener);
            return tweener;
        }

        public Tweener<Color> ToColor(object target, Func<Color> getter, Action<Color> setter, Color endValue, int duration)
        {
            var tweener = Tweener<Color>.Create(target, getter, setter, endValue, duration, TweenLerp.LerpColor, this);
            pendingAdd.Enqueue(tweener);
            return tweener;
        }

        public Tweener<Quaternion> ToQuaternion(object target, Func<Quaternion> getter, Action<Quaternion> setter, Quaternion endValue, int duration)
        {
            var tweener = Tweener<Quaternion>.Create(target, getter, setter, endValue, duration, TweenLerp.LerpQuaternion, this);
            pendingAdd.Enqueue(tweener);
            return tweener;
        }

        public Sequence CreateSequence()
        {
            var seq = Sequence.Create(this);
            pendingAdd.Enqueue(seq);
            return seq;
        }

        internal void AddTween(Tween tween)
        {
            pendingAdd.Enqueue(tween);
        }

        #endregion

        #region Query & Control

        public bool IsTweening(object target)
        {
            if (target == null) return false;
            return targetTweens.TryGetValue(target, out var list) && list.Count > 0;
        }

        public int KillTweens(object target)
        {
            if (target == null) return 0;
            if (!targetTweens.TryGetValue(target, out var list)) return 0;

            int count = list.Count;
            for (int i = 0; i < list.Count; i++)
            {
                list[i].Kill(false);
            }
            list.Clear();
            targetTweens.Remove(target);
            return count;
        }

        public int KillAll()
        {
            int count = activeTweens.Count;
            foreach (var tween in activeTweens)
            {
                tween.Kill(false);
                tween.Reset();
            }
            activeTweens.Clear();
            targetTweens.Clear();
            return count;
        }

        #endregion
    }
}
