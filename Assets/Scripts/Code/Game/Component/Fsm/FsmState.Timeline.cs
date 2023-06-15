using System.Collections.Generic;

namespace TaoTie
{
    public partial class FsmState
    {
        private int clipIndex = 0;
        private float nextClipTime = 0.0f;
        private ConfigFsmTimeline timeline = null;
        private LinkedListComponent<FsmClip> clipList = null;

        private void StartTimeline()
        {
            if (!config.HasTimeline)
            {
                clipIndex = -1;
            }
            else
            {
                clipIndex = 0;
                timeline = config.Timeline;
                nextClipTime = timeline.Clips[0].StartTime;
                clipList = LinkedListComponent<FsmClip>.Create();
                CheckNextClip();
            }
        }

        private void UpdateTimeline()
        {
            UpdateClip();
            CheckNextClip();
        }

        private void StopTimeline()
        {
            if (timeline!=null && timeline.Clips != null)
            {
                for (int i = 0; i < timeline.Clips.Length; i++)
                {
                    if (timeline.Clips[i].StartTime > StateTime)
                    {
                        FsmClip clip = timeline.Clips[i].CreateClip(this);
                        clip.Break(StateTime);
                        clip.Dispose();
                    }
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

        private void CheckNextClip()
        {
            if (clipIndex < 0)
                return;

            while (StateTime >= nextClipTime)
            {
                ConfigFsmClip clipConfig = timeline.Clips[clipIndex];
                FsmClip clip = clipConfig.CreateClip(this);
                if (clip != null)
                {
                    clip.Start(StateTime);
                    clipList.AddLast(clip);
                }

                ++clipIndex;
                if (clipIndex < timeline.Clips.Length)
                {
                    nextClipTime = timeline.Clips[clipIndex].StartTime;
                }
                else
                {
                    clipIndex = -1;
                    return;
                }
            }
        }

        private void ClearTimeline()
        {
            if (clipList != null)
            {
                clipList.Dispose();
                clipList = null;
            }
            timeline = null;
        }
    }
}