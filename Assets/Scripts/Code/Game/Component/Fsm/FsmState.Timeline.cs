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
            if (!config.hasTimeline)
            {
                clipIndex = -1;
            }
            else
            {
                clipIndex = 0;
                timeline = config.timeline;
                nextClipTime = timeline.clips[0].StartTime;
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

            var nowtime = fsm.stateTime;
            var elapseTime = fsm.stateElapseTime;
            for (var node = clipList.First; node!=null; )
            {
                var next = node.Next;
                if (node.Value != null)
                {
                    node.Value.Update(nowtime, elapseTime);
                    if (!node.Value.isPlaying)
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

            while (fsm.stateTime >= nextClipTime)
            {
                ConfigFsmClip clipConfig = timeline.clips[clipIndex];
                FsmClip clip = clipConfig.CreateClip(this);
                if (clip != null)
                {
                    clip.Start(fsm.stateTime);
                    clipList.AddLast(clip);
                }

                ++clipIndex;
                if (clipIndex < timeline.clips.Length)
                {
                    nextClipTime = timeline.clips[clipIndex].StartTime;
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