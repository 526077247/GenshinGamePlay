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
        [Timer(TimerType.ResetTimeScale)]
        public class ResetTimeScale : ATimer<GameTimerManager>
        {
            public override void Run(GameTimerManager t)
            {
                try
                {
                    t.SetTimeScale(1);
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
            }
        }
        public static GameTimerManager Instance  { get; private set; }

        /// <summary> 时间缩放,标准值为1 </summary>
        private float timeScale = 1;

        private long lastUpdateTime;
        
        private long deltaTime;
        
        private long timeNow;

        private long resetTimeScaleId;

        /// <summary>
        /// 时停到期的<b>真实时间</b>戳(ms)，0 表示当前没有时停排期。
        /// 不能像其他定时器那样把到期点挂在 <see cref="timeNow"/> 上：那是被 timeScale 缩放后的
        /// 游戏时间，时停期间几乎不推进（见 Update 里 deltaTime 的取整），会让「时停 75ms」变成一两秒。
        /// </summary>
        private long resetRealTime;
        #region override
        
        public override void Init()
        {
            Instance = this;
            // todo:从服务器或存档中取当前时间
            timeNow = 0;
            lastUpdateTime = TimeInfo.Instance.ClientNow();
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
            var clientNow = TimeInfo.Instance.ClientNow();

            // 时停恢复必须按真实时间判断，且必须在下面 timeScale<=0 的提前返回之前：
            // 低 timeScale 下 timeNow 几乎不推进（deltaTime=(int)(changeTime*scale)，
            // 16ms×0.05=0.8 截断成 0），恢复定时器会被拉长到一两秒；
            // 若 HitHaltTimeScale 配成 0，timeNow 永不推进 → 恢复定时器永不触发 → 全局永久卡死。
            if (resetRealTime > 0 && clientNow >= resetRealTime)
            {
                SetTimeScale(1);
            }

            long changeTime = clientNow - lastUpdateTime;
            deltaTime = (int) (changeTime * timeScale);
            if (timeScale <= 0)
            {
                lastUpdateTime = clientNow;
                return;
            }

            lastUpdateTime = Math.Min(clientNow, lastUpdateTime + (int) (deltaTime / timeScale));
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

        /// <summary>
        /// 重复设置会覆盖
        /// </summary>
        /// <param name="scale"></param>
        /// <param name="during"></param>
        public void SetTimeScale(float scale, int during = -1)
        {
            if(scale<0) return;
            timeScale = scale;
            TimerManager.Instance.Remove(ref resetTimeScaleId);
            resetRealTime = 0;
            if (during >= 0)
            {
                // 时停时长按真实时间计（游戏时间在时停期间几乎不推进，见 Update 注释）。
                resetRealTime = TimeInfo.Instance.ClientNow() + during;
            }
            Messager.Instance.Broadcast(0, MessageId.TimeScaleChange, timeScale);
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