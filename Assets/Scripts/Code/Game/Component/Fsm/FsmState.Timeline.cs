using System.Collections.Generic;

namespace TaoTie
{
    public partial class FsmState
    {
        /// <summary>timeline.Clips 的最大可用下标数（位图宽度）。</summary>
        private const int MaxClips = 64;

        /// <summary>
        /// 已触发 clip 的位图，bit i 对应 <c>timeline.Clips[i]</c>。
        ///
        /// 不能用「下一个待触发下标」线性推进：导出产物里 <c>Clips</c> 的顺序取决于 Unity 侧
        /// Timeline 轨道的保存顺序，实测出现过倒序（NormalAttack4 导成
        /// <c>[Show@1.41, Hide@0.65, NA1@0.31, NA1@0.14]</c>）。线性推进会在 StateTime 越过 1.41 时
        /// 先触发 Show、同一帧紧接着触发 Hide，成对 Hide/Show 彻底反转 —— 表现为「虚化后再也不出现」。
        /// 位图 + 每次挑「StartTime 最小且未触发」与数组顺序无关，倒序 / 乱序都能按时间正确播。
        /// </summary>
        private ulong clipTriggered;

        private ConfigFsmTimeline timeline = null;
        private LinkedListComponent<FsmClip> clipList = null;

        private bool HasTimeline => timeline != null && timeline.Clips != null;

        private int ClipCount
        {
            get
            {
                if (!HasTimeline) return 0;
                return timeline.Clips.Length < MaxClips ? timeline.Clips.Length : MaxClips;
            }
        }

        private bool IsTriggered(int index) => (clipTriggered & (1UL << index)) != 0;

        private void StartTimeline()
        {
            if (!config.HasTimeline)
            {
                return;
            }

            timeline = config.Timeline;
            clipTriggered = 0;
            clipList = LinkedListComponent<FsmClip>.Create();
            if (timeline.Clips != null && timeline.Clips.Length > MaxClips)
            {
                Log.Error($"FsmState {Name} 的 clip 数超过 {MaxClips}，超出的 {timeline.Clips.Length - MaxClips} 个不会触发");
            }
            CheckNextClip();
        }

        private void UpdateTimeline()
        {
            UpdateClip();
            CheckNextClip();
        }

        private void StopTimeline()
        {
            if (HasTimeline)
            {
                // 只补执行「还没触发过」的 clip：已触发的由下面的 Stop 收尾，
                // 这里再 Break 一次就会变成一次打断两次收尾。
                var clips = timeline.Clips;
                int count = ClipCount;
                for (int i = 0; i < count; i++)
                {
                    if (IsTriggered(i)) continue;
                    FsmClip clip = FsmSystem.Instance.CreateFsmClip(clips[i], this);
                    if (clip == null) continue;
                    clip.Break(StateTime);
                    clip.Dispose();
                }
            }

            if (clipList == null || clipList.Count <= 0)
                return;

            for (var node = clipList.First; node!=null; node = node.Next)
            {
                if (node.Value != null)
                {
                    node.Value.Stop();
                    node.Value.Dispose();
                }
            }
            clipList.Clear();
        }

        private void UpdateClip()
        {
            if (clipList == null || clipList.Count == 0)
                return;

            var nowtime = StateTime;
            var elapseTime = StateElapseTime;
            for (var node = clipList.First; node!=null; )
            {
                var next = node.Next;
                if (node.Value != null)
                {
                    node.Value.Update(nowtime, elapseTime);
                    if (!node.Value.IsPlaying)
                    {
                        node.Value.Dispose();
                        clipList.Remove(node);
                    }
                }

                node = next;
            }
        }

        /// <summary>
        /// 触发所有「StartTime 已到且尚未触发」的 clip，按 StartTime 升序逐个执行，
        /// 与 <c>timeline.Clips</c> 的数组顺序无关（原因见 <see cref="clipTriggered"/>）。
        /// </summary>
        private void CheckNextClip()
        {
            if (!HasTimeline) return;

            var clips = timeline.Clips;
            int count = ClipCount;
            while (true)
            {
                int pick = -1;
                float pickTime = float.MaxValue;
                for (int i = 0; i < count; i++)
                {
                    if (IsTriggered(i)) continue;
                    float startTime = clips[i].StartTime;
                    if (startTime <= StateTime && startTime < pickTime)
                    {
                        pick = i;
                        pickTime = startTime;
                    }
                }

                if (pick < 0) return;

                clipTriggered |= 1UL << pick;
                FsmClip clip = FsmSystem.Instance.CreateFsmClip(clips[pick], this);
                if (clip != null)
                {
                    clip.Start(StateTime);
                    clipList.AddLast(clip);
                }
            }
        }

        private void ClearTimeline()
        {
            if (clipList != null)
            {
                // LinkedListComponent.Dispose() 只做 Clear + 回收容器本身，不回收元素。
                for (var node = clipList.First; node != null; node = node.Next)
                {
                    node.Value?.Dispose();
                }
                clipList.Dispose();
                clipList = null;
            }
            timeline = null;
            clipTriggered = 0;
        }
    }
}
