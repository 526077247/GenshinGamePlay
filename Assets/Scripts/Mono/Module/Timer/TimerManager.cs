using System;
using System.Collections.Generic;
using System.Reflection;

namespace TaoTie
{
    public enum TimerClass:byte
    {
        None,
        OnceTimer,
        OnceWaitTimer,
        RepeatedTimer,
    }
    
    public class TimerManager:IUpdateManager,IManager
    {

        public static TimerManager Instance { get; private set; }

        protected Dictionary<long, TimerAction> childs = new Dictionary<long, TimerAction>();
        /// <summary>
        /// key: time, value: timer id
        /// </summary>
        protected readonly MultiMap<long, long> TimeId = new MultiMap<long, long>();

        protected readonly Queue<long> timeOutTime = new Queue<long>();

        protected readonly Queue<long> timeOutTimerIds = new Queue<long>();
        
        protected readonly Queue<long> everyFrameTimer = new Queue<long>();

        // 记录最小时间，不用每次都去MultiMap取第一个值
        protected long minTime;

        protected const int TimeTypeMax = 10000;

        protected ITimer[] timerActions;

        #region override

        public virtual void Init()
        {
            Instance = this;
            
            InitAction();
        }

        public virtual void Destroy()
        {
            Instance = null;
            foreach (var item in this.childs)
            {
                item.Value.Dispose();
            }
            this.childs.Clear();
        }

        public virtual void Update()
        {
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

            var timeNow = GetTimeNow();

            if (timeNow < this.minTime)
            {
                return;
            }

            foreach (var item in this.TimeId)
            {
                if (item.Key > timeNow)
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

        protected void InitAction()
        {
            this.timerActions = new ITimer[TimerManager.TimeTypeMax];
            List<Type> types = AttributeManager.Instance.GetTypes(TypeInfo<TimerAttribute>.Type);

            foreach (Type type in types)
            {
                ITimer iTimer = Activator.CreateInstance(type) as ITimer;
                if (iTimer == null)
                {
                    Log.Error($"Timer Action {type.Name} 需要继承 ITimer");
                    continue;
                }
                
                object[] attrs = type.GetCustomAttributes(TypeInfo<TimerAttribute>.Type, false);
                if (attrs.Length == 0)
                {
                    continue;
                }

                foreach (object attr in attrs)
                {
                    TimerAttribute timerAttribute = attr as TimerAttribute;
                    this.timerActions[timerAttribute.Type] = iTimer;
                }
            }
        }
        
        protected void Run(TimerAction timerAction)
        {
            switch (timerAction.TimerClass)
            {
                case TimerClass.OnceTimer:
                {
                    int type = timerAction.Type;
                    ITimer timer = this.timerActions[type];
                    if (timer == null)
                    {
                        Log.Error($"not found timer action: {type}");
                        return;
                    }
                    object obj = timerAction.Object;
                    Remove(timerAction.Id);
                    timer.Handle(obj);
                    break;
                }
                case TimerClass.OnceWaitTimer:
                {
                    ETTask<bool> tcs = timerAction.Object as ETTask<bool>;
                    this.Remove(timerAction.Id);
                    tcs.SetResult(true);
                    break;
                }
                case TimerClass.RepeatedTimer:
                {
                    int type = timerAction.Type;
                    long tillTime = GetTimeNow() + timerAction.Time;
                    this.AddTimer(tillTime, timerAction);

                    ITimer timer = this.timerActions[type];
                    if (timer == null)
                    {
                        Log.Error($"not found timer action: {type}");
                        return;
                    }
                    timer.Handle(timerAction.Object);
                    break;
                }
            }
        }
        
        private void AddTimer(long tillTime, TimerAction timer)
        {
            if (timer.TimerClass == TimerClass.RepeatedTimer && timer.Time == 0)
            {
                this.everyFrameTimer.Enqueue(timer.Id);
                return;
            }
            this.TimeId.Add(tillTime, timer.Id);
            if (tillTime < this.minTime)
            {
                this.minTime = tillTime;
            }
        }

        public bool Remove(ref long id)
        {
            long i = id;
            id = 0;
            return this.Remove(i);
        }
        
        private bool Remove(long id)
        {
            if (id == 0)
            {
                return false;
            }

            TimerAction timerAction = this.GetChild(id);
            if (timerAction == null)
            {
                return false;
            }

            this.RemoveChild(id);
            return true;
        }

        public async ETTask<bool> WaitTillAsync(long tillTime, ETCancellationToken cancellationToken = null)
        {
            if (this.GetTimeNow() >= tillTime)
            {
                return true;
            }

            ETTask<bool> tcs = ETTask<bool>.Create(true);
            TimerAction timer = this.AddChild(TimerClass.OnceWaitTimer, tillTime - this.GetTimeNow(), 0, tcs);
            this.AddTimer(tillTime, timer);
            long timerId = timer.Id;

            void CancelAction()
            {
                if (this.Remove(timerId))
                {
                    tcs.SetResult(false);
                }
            }
            
            bool ret;
            try
            {
                cancellationToken?.Add(CancelAction);
                ret = await tcs;
            }
            catch (Exception ex)
            {
                ret = false;
                Log.Error(ex);
            }
            finally
            {
                cancellationToken?.Remove(CancelAction);    
            }
            return ret;
        }

        public async ETTask<bool> WaitFrameAsync(ETCancellationToken cancellationToken = null)
        {
            bool ret = await this.WaitAsync(1, cancellationToken);
            return ret;
        }

        public async ETTask<bool> WaitAsync(long time, ETCancellationToken cancellationToken = null)
        {
            if (time == 0)
            {
                return true;
            }
            long tillTime = GetTimeNow() + time;

            ETTask<bool> tcs = ETTask<bool>.Create(true);
            
            TimerAction timer = this.AddChild(TimerClass.OnceWaitTimer, time, 0, tcs);
            this.AddTimer(tillTime, timer);
            long timerId = timer.Id;
            void CancelAction()
            {
                if (this.Remove(timerId))
                {
                    tcs.SetResult(false);
                }
            }

            bool ret;
            try
            {
                cancellationToken?.Add(CancelAction);
                ret = await tcs;
            }
            catch (Exception ex)
            {
                ret = false;
                Log.Error(ex);
            }
            finally
            {
                cancellationToken?.Remove(CancelAction); 
            }
            return ret;
        }
        
        // 用这个优点是可以热更，缺点是回调式的写法，逻辑不连贯。WaitTillAsync不能热更，优点是逻辑连贯。
        // wait时间短并且逻辑需要连贯的建议WaitTillAsync
        // wait时间长不需要逻辑连贯的建议用NewOnceTimer
        public long NewOnceTimer(long tillTime, int type, object args)
        {
            if (tillTime < GetTimeNow())
            {
                Log.Warning($"new once time too small: {tillTime}");
            }
            TimerAction timer = this.AddChild(TimerClass.OnceTimer, tillTime, type, args);
            this.AddTimer(tillTime, timer);
            return timer.Id;
        }

        public long NewFrameTimer(int type, object args)
        {

            return this.NewRepeatedTimerInner(0, type, args);

        }

        public virtual long GetTimeNow()
        {
            return TimeHelper.ServerNow();
        }

        /// <summary>
        /// 创建一个RepeatedTimer
        /// </summary>
        private long NewRepeatedTimerInner(long time, int type, object args)
        {
            long tillTime = GetTimeNow() + time;
            TimerAction timer = this.AddChild(TimerClass.RepeatedTimer, time, type, args);

            // 每帧执行的不用加到timerId中，防止遍历
            this.AddTimer(tillTime, timer);
            return timer.Id;
        }

        public long NewRepeatedTimer(long time, int type, object args)
        {
            if (time < 100)
            {
                Log.Error($"time too small: {time}");
                return 0;
            }
            return this.NewRepeatedTimerInner(time, type, args);
        }
        
        public TimerAction AddChild(TimerClass timerClass, long time, int type, object obj)
        {
            TimerAction timer =TimerAction.Create(timerClass,time,type,obj);
            this.childs.Add(timer.Id,timer);
            return timer;
        }

        public TimerAction GetChild(long id)
        {
            TimerAction res = null;
            this.childs.TryGetValue(id, out res);
            return res;
        }

        public void RemoveChild(long id)
        {
            var timer = this.GetChild(id);
            if (timer != null)
            {
                this.childs.Remove(id);
                timer.Dispose();
            }
        }
    }
}