using System.Collections.Generic;

namespace TaoTie
{
    public partial class FsmState
    {
        private int _clipIndex = 0;
        private float _nextClipTime = 0.0f;
        private ConfigFsmTimeline _timeline = null;
        private LinkedListComponent<FsmClip> _clipList = null;

        private void StartTimeline()
        {
            if (!_config.hasTimeline)
            {
                _clipIndex = -1;
            }
            else
            {
                _clipIndex = 0;
                _timeline = _config.timeline;
                _nextClipTime = _timeline.clips[0].starttime;
                _clipList = LinkedListComponent<FsmClip>.Create();
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
            if (_clipList == null || _clipList.Count <= 0)
                return;

            for (var node = _clipList.First; node!=null; node = node.Next)
            {
                if (node.Value != null)
                {
                    node.Value.Stop();
                    node.Value.Dispose();
                }
            }
            _clipList.Clear();
        }

        private void UpdateClip()
        {
            if (_clipList == null || _clipList.Count == 0)
                return;

            var nowtime = _fsm.stateTime;
            var elapseTime = _fsm.stateElapseTime;
            for (var node = _clipList.First; node!=null; )
            {
                var next = node.Next;
                if (node.Value != null)
                {
                    node.Value.Update(nowtime, elapseTime);
                    if (!node.Value.isPlaying)
                    {
                        node.Value.Dispose();
                        _clipList.Remove(node);
                    }
                }

                node = next;
            }
        }

        private void CheckNextClip()
        {
            if (_clipIndex < 0)
                return;

            while (_fsm.stateTime >= _nextClipTime)
            {
                ConfigFsmClip clipConfig = _timeline.clips[_clipIndex];
                FsmClip clip = clipConfig.CreateClip(this);
                if (clip != null)
                {
                    clip.Start(_fsm.stateTime);
                    _clipList.AddLast(clip);
                }

                ++_clipIndex;
                if (_clipIndex < _timeline.clips.Length)
                {
                    _nextClipTime = _timeline.clips[_clipIndex].starttime;
                }
                else
                {
                    _clipIndex = -1;
                    return;
                }
            }
        }

        private void ClearTimeline()
        {
            if (_clipList != null)
            {
                _clipList.Dispose();
                _clipList = null;
            }
            _timeline = null;
        }
    }
}