﻿using UnityEngine;

namespace TaoTie
{
    public class BlenderCameraState: CameraState
    {
        public override bool IsBlenderState => true;
        private ConfigBlender config;

        public NormalCameraState From { get; private set; }
        public NormalCameraState To { get; private set; }

        private CameraStateData formData;
        private CameraStateData toData;

        private EasingFunction.Function lerpFunc;

        private long startlerpTime;
        
        public static BlenderCameraState Create(NormalCameraState from, NormalCameraState to,bool isEnter)
        {
            BlenderCameraState res = ObjectPool.Instance.Fetch<BlenderCameraState>();
            res.Id = IdGenerater.Instance.GenerateId();
            res.From = from;
            res.To = to;
            res.formData = res.From.Data;
            res.toData = res.To.Data;
            res.Data = CameraStateData.DeepClone(res.From.Data);
            res.Priority = to.Priority;
            if (isEnter)
            {
                res.config = to.Config.Enter;
            }
            else
            {
                res.config = from.Config.Leave;
            }
            if (res.config == null)
                res.config = CameraManager.Instance.DefaultBlend;
            
            res.IsOver = false;
            res.lerpFunc = EasingFunction.GetEasingFunction(res.config.Ease);
            res.startlerpTime = TimerManager.Instance.GetTimeNow();
            return res;
        }
        public override void OnEnter()
        {
            base.OnEnter();
            CameraManager.Instance.ChangeCursorVisible(To.Config.VisibleCursor, CursorStateType.Camera);
            CameraManager.Instance.ChangeCursorLock(To.Config.UnLockCursor, CursorStateType.Camera);
        }
        
        public override void Update()
        {
            if (To.IsOver) IsOver = true;
            if (IsOver) return;
            var timeNow = TimerManager.Instance.GetTimeNow();
            if (timeNow >= startlerpTime + config.DeltaTime)
            {
                IsOver = true;
            }

            float lerpVal;
            if (config.DeltaTime > 0)
            {
                lerpVal = lerpFunc((float) (timeNow - startlerpTime) / config.DeltaTime, 0, 1);
                lerpVal = Mathf.Clamp01(lerpVal);
            }
            else
            {
                lerpVal = 1;
            }
            Data.Lerp(formData, toData, lerpVal);
        }

        /// <summary>
        /// 切换过渡目标
        /// </summary>
        /// <param name="to"></param>
        /// <param name="isEnter"></param>
        public void ChangeTo(NormalCameraState to,bool isEnter)
        {
            this.To = to;
            formData = Data;
            Data = CameraStateData.DeepClone(formData);
            toData = to.Data;
            if (isEnter)
            {
                config = to.Config.Enter;
            }
            else
            {
                config = From.Config.Leave;
            }

            if (config == null)
                config = CameraManager.Instance.DefaultBlend;
            Priority = to.Priority;
            IsOver = false;
            lerpFunc = EasingFunction.GetEasingFunction(config.Ease);
            startlerpTime = TimerManager.Instance.GetTimeNow();
            CameraManager.Instance.ChangeCursorVisible(To.Config.VisibleCursor, CursorStateType.Camera);
            CameraManager.Instance.ChangeCursorLock(To.Config.UnLockCursor, CursorStateType.Camera);
        }

        public override void Dispose()
        {
            base.Dispose();
            
            //this
            if (From.IsOver)
            {
                From.Dispose();
            }
            From = null;
            To = null;
            config = null;
            lerpFunc = null;
            startlerpTime = default;
            ObjectPool.Instance.Recycle(this);
        }
    }
}