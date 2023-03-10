using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

namespace TaoTie
{
    public class CameraShakeManager
    {
        private static CameraShakeManager sInstance;
        private readonly List<CinemachineScreenShakeEvent> _cameraShakeList;
        private CinemachineScreenShakeEvent _currentShakeEvent;

        private CinemachineScreenShakeEvent _largestShakeEventThisFrame;

        private Vector3 _shakeVelocity = Vector3.zero;

        private CameraShakeManager()
        {
            _cameraShakeList = new List<CinemachineScreenShakeEvent>();
        }

        private float currentTime => Time.time;

        public static CameraShakeManager Instance
        {
            get
            {
                if (sInstance == null)
                    sInstance = new CameraShakeManager();
                return sInstance;
            }
        }

        public void ActShakeEvent(CinemachineScreenShakeEvent e, bool clearPreviousShake = false)
        {
            if (_largestShakeEventThisFrame == null || e.priority > _largestShakeEventThisFrame.priority)
            {
                e.StartTime = currentTime;
                var index = SeekListAddPosition();
                _cameraShakeList[index] = e;
                if (SeekLagestEntryIndex() == index || clearPreviousShake)
                {
                    if (clearPreviousShake) _cameraShakeList.Clear();
                    SetupShake(e);
                }

                _largestShakeEventThisFrame = e;
            }
        }

        public void ActShakeEvent(float delay, float shakeRange, Vector3 shakeDir, float duration,
            float cycleTime = 0.05f)
        {
            var shakeSource = new CameraShakeSource(delay, shakeRange, shakeDir, cycleTime, duration);
            var e =
                new CinemachineScreenShakeEvent(shakeSource);
            Instance.ActShakeEvent(e);
        }

        public void ActShakeEvent(ISignalSource6D signalSource)
        {
            ActShakeEvent(new CinemachineScreenShakeEvent(signalSource));
        }


        private void SetupShake(CinemachineScreenShakeEvent e)
        {
            _currentShakeEvent = e;
            var source = _currentShakeEvent.ShakeSource as CameraShakeSource;
            _shakeVelocity = source.velocity;
            Debug.Log("Current Shake Event is : " + _currentShakeEvent);
        }

        public bool GetShake(out Vector3 pos, out Quaternion rot)
        {
            pos = Vector3.zero;
            rot = Quaternion.identity;
            var nontrivialResult = false;

            if (_currentShakeEvent != null)
            {
                nontrivialResult = true;

                var source = _currentShakeEvent.ShakeSource as CameraShakeSource;
                source.velocity = _shakeVelocity * Mathf.Lerp(_currentShakeEvent.timer / source.duration, 0.01f, 0.8f);

                Vector3 pos0;
                Quaternion rot0;
                _currentShakeEvent.GetSignal(out pos0, out rot0);
                pos += pos0;
                rot *= rot0;

                if (_currentShakeEvent.timer < 0) _currentShakeEvent = null;
            }


            var flag = false;
            for (var i = 0; i < _cameraShakeList.Count; i++)
            {
                if (_cameraShakeList[i] == null)
                    continue;
                var e = _cameraShakeList[i];
                if (e.timer <= 0)
                {
                    _cameraShakeList[i] = null;
                    flag = true;
                }

                if (flag)
                {
                    Debug.Log("Timer is zero");
                    var inedex = SeekLagestEntryIndex();
                    if (inedex >= 0) SetupShake(_cameraShakeList[inedex]);
                }
            }

            _largestShakeEventThisFrame = null;
            return nontrivialResult;
        }


        //找寻插入点
        private int SeekListAddPosition()
        {
            for (var i = 0; i < _cameraShakeList.Count; i++)
                if (_cameraShakeList[i] == null)
                    return i;
            _cameraShakeList.Add(null);
            return _cameraShakeList.Count - 1;
        }

        //寻找权重最大的Shake Entry
        private int SeekLagestEntryIndex()
        {
            var index = -1;
            float priority = 0;
            Debug.Log("Shake list count is : " + _cameraShakeList.Count);
            for (var i = 0; i < _cameraShakeList.Count; i++)
                if (_cameraShakeList[i] != null)
                {
                    //比较权重，由Range、生命时间比例决定
                    var entry = _cameraShakeList[i].ShakeSource as CameraShakeSource;
                    if (entry.shakeRange * (_cameraShakeList[i].timer / entry.SignalDuration) > priority)
                    {
                        priority = entry.shakeRange * (_cameraShakeList[i].timer / entry.SignalDuration);
                        if (_currentShakeEvent != null)
                            if (entry.shakeRange == _currentShakeEvent.priority)
                                continue;
                        index = i;
                    }
                }

            Debug.Log("Largest shake index is : " + index);
            return index;
        }


        public class CinemachineScreenShakeEvent
        {
            public ISignalSource6D ShakeSource;
            public float StartTime;

            public CinemachineScreenShakeEvent()
            {
            }

            public CinemachineScreenShakeEvent(ISignalSource6D source)
            {
                ShakeSource = source;
            }

            public float timer => ShakeSource.SignalDuration + StartTime - Instance.currentTime;

            public float priority
            {
                get
                {
                    var source = ShakeSource as CameraShakeSource;
                    return source.shakeRange;
                }
            }

            public void GetSignal(out Vector3 pos, out Quaternion rot)
            {
                ShakeSource.GetSignal(Instance.currentTime - StartTime, out pos, out rot);
            }
        }
    }
}