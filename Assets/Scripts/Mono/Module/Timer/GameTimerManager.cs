using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 游戏时间,受时间缩放影响
    /// </summary>
    public class GameTimerManager:TimerManager
    {
        public static GameTimerManager Instance  { get; private set; }

        /// <summary> 时间缩放,标准值为1 </summary>
        private float timeScale = 1;

        private long lastUpdateTime;
        
        private long deltaTime;
        
        private long timeNow;
        #region override
        
        public override void Init()
        {
            Instance = this;
            // todo:从服务器或存档中取当前时间
            timeNow = 0;
            lastUpdateTime = TimerManager.Instance.GetTimeNow();
            InitAction();
        }

        public override void Destroy()
        {
            Instance = null;
            foreach (var item in this.childs)
            {
                item.Value.Dispose();
            }
            this.childs.Clear();
        }
        
        public override void Update()
        {
            var serverNow = TimerManager.Instance.GetTimeNow();
            deltaTime = (int)((serverNow - lastUpdateTime)*timeScale);
            lastUpdateTime = serverNow;
            if(timeScale<=0) return;
            timeNow += deltaTime;
            #region 每帧执行的timer，不用foreach TimeId，减少GC

            int count = this.everyFrameTimer.Count;
            for (int i = 0; i < count; ++i)
            {
                long timerId = this.everyFrameTimer.Dequeue();
                TimerAction timerAction = this.GetChild(timerId);
                if (timerAction == null)
                {
                    continue;
                }

                this.Run(timerAction);
            }

            #endregion

            if (this.TimeId.Count == 0)
            {
                return;
            }

            if (this.timeNow < this.minTime)
            {
                return;
            }

            foreach (var item in this.TimeId)
            {
                if (item.Key > this.timeNow)
                {
                    this.minTime = item.Key;
                    break;
                }

                this.timeOutTime.Enqueue(item.Key);
            }

            while (this.timeOutTime.Count > 0)
            {
                long time = this.timeOutTime.Dequeue();
                var list = this.TimeId[time];
                for (int i = 0; i < list.Count; ++i)
                {
                    long timerId = list[i];
                    this.timeOutTimerIds.Enqueue(timerId);
                }

                this.TimeId.Remove(time);
            }

            while (this.timeOutTimerIds.Count > 0)
            {
                long timerId = this.timeOutTimerIds.Dequeue();

                TimerAction timerAction = this.GetChild(timerId);
                if (timerAction == null)
                {
                    continue;
                }

                this.Run(timerAction);
            }
        }
        
        #endregion

        public void SetTimeScale(float scale)
        {
            if(scale<0) return;
            timeScale = scale;
            Messager.Instance.Broadcast(0,MessageId.TimeScaleChange,timeScale);
        }
        
        public float GetTimeScale()
        {
            return timeScale;
        }

        public override long GetTimeNow()
        {
            return timeNow;
        }
        
        public long GetDeltaTime()
        {
            return deltaTime;
        }
    }
}