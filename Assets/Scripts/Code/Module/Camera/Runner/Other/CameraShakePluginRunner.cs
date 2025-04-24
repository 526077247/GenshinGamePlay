using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    public sealed class CameraShakePluginRunner: CameraOtherPluginRunner<ConfigCameraShakePlugin>
    {
        private List<CameraShakeParam> paramList;
        private CameraShakeParam current;
        private long curTime;
        private Vector3 curDir;
        protected override void InitInternal()
        {
            paramList = new List<CameraShakeParam>();
            Messager.Instance.AddListener<CameraShakeParam>(0, MessageId.ShakeCamera, OnShakeCamera);
        }

        protected override void DisposeInternal()
        {
            Messager.Instance.RemoveListener<CameraShakeParam>(0, MessageId.ShakeCamera, OnShakeCamera);
            paramList = null;
        }

        protected override void UpdateInternal()
        {
            var timeNow = GameTimerManager.Instance.GetTimeNow();
            CameraShakeParam maxRange = null;
            for (int i = paramList.Count-1; i >=0; i--)
            {
                var param = paramList[i];
                if (timeNow > param.StartTime + param.ShakeTime)
                {
                    paramList.RemoveAt(i);
                    continue;
                }

                if (maxRange == null || param.ShakeRange > maxRange.ShakeRange)
                {
                    maxRange = param;
                }
            }
    
            if (maxRange != current)
            {
                curTime = 0;
                current = maxRange;
            }

            if (current == null)
            {
                curDir = Vector3.Lerp(curDir, Vector3.zero, 0.7f);
            }
            else
            {
                curDir = current.ShakeDir * (current.ShakeRange *
                         Mathf.Sin((timeNow - current.StartTime) * 2 * Mathf.PI * current.ShakeFrequency / 1000));
            }
            data.Position += curDir;
        }

        private void OnShakeCamera(CameraShakeParam param)
        {
            for (int i = 0; i < paramList.Count; i++)
            {
                if(paramList[i].Id == param.Id) return;
            }
            var dis = Vector3.Distance(param.Source, data.Position);
            if (dis > param.ShakeDistance) return;
            if (dis > param.ShakeDistance - param.RangeAttenuation)
            {
                param.ShakeRange *= (param.ShakeDistance - dis) / param.RangeAttenuation;
            }
            param.StartTime = GameTimerManager.Instance.GetTimeNow();
            paramList.Add(param);
        }
    }
}