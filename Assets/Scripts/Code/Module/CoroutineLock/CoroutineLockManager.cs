using System;
using System.Collections.Generic;
namespace TaoTie
{
    public class CoroutineLockManager:IUpdateManager
    {

        public static CoroutineLockManager Instance { get; private set; }
        public List<CoroutineLockQueueType> list;
        public Queue<(int, long, int)> nextFrameRun = new Queue<(int, long, int)>();
        public MultiMap<long, CoroutineLockTimer> timers = new MultiMap<long, CoroutineLockTimer>();
        public Queue<long> timeOutIds = new Queue<long>();
        public Queue<CoroutineLockTimer> timerOutTimer = new Queue<CoroutineLockTimer>();
        public long minTime;
        public long timeNow;

        #region override

        public void Init()
        {
            Instance = this;
            this.list = new List<CoroutineLockQueueType>(CoroutineLockType.Max);
            for (int i = 0; i < CoroutineLockType.Max; ++i)
            {
                CoroutineLockQueueType coroutineLockQueueType = CoroutineLockQueueType.Create();
                this.list.Add(coroutineLockQueueType);
            }
        }

        public void Destroy()
        {
            Instance = null;
            for (int i = 0; i < list.Count; i++)
            {
                list[i].Dispose();
            }
            this.list.Clear();
            this.nextFrameRun.Clear();
            this.timers.Clear();
            this.timeOutIds.Clear();
            this.timerOutTimer.Clear();
            this.minTime = 0;
        }

        public void Update()
        {
            // 检测超时的CoroutineLock
            TimeoutCheck(this);

            // 循环过程中会有对象继续加入队列
            while (this.nextFrameRun.Count > 0)
            {
                (int coroutineLockType, long key, int count) = this.nextFrameRun.Dequeue();
                this.Notify(coroutineLockType, key, count);
            }
        }
        #endregion
        
        private void TimeoutCheck(CoroutineLockManager self)
        {
            // 超时的锁
            if (self.timers.Count == 0)
            {
                return;
            }

            self.timeNow = TimeHelper.ClientFrameTime();

            if (self.timeNow < self.minTime)
            {
                return;
            }

            foreach (var item in self.timers)
            {
                if (item.Key > self.timeNow)
                {
                    self.minTime = item.Key;
                    break;
                }

                self.timeOutIds.Enqueue(item.Key);
            }

            self.timerOutTimer.Clear();

            while (self.timeOutIds.Count > 0)
            {
                long time = self.timeOutIds.Dequeue();
                var list = self.timers[time];
                for (int i = 0; i < list.Count; ++i)
                {
                    CoroutineLockTimer coroutineLockTimer = list[i];
                    self.timerOutTimer.Enqueue(coroutineLockTimer);
                }

                self.timers.Remove(time);
            }

            while (self.timerOutTimer.Count > 0)
            {
                CoroutineLockTimer coroutineLockTimer = self.timerOutTimer.Dequeue();
                if (coroutineLockTimer.CoroutineLockInstanceId != coroutineLockTimer.CoroutineLock.InstanceId)
                {
                    continue;
                }

                CoroutineLock coroutineLock = coroutineLockTimer.CoroutineLock;
                // 超时直接调用下一个锁
                self.RunNextCoroutine(coroutineLock.coroutineLockType, coroutineLock.key, coroutineLock.level + 1);
                coroutineLock.coroutineLockType = CoroutineLockType.None; // 上面调用了下一个, dispose不再调用
            }
        }
        
        public void RunNextCoroutine(int coroutineLockType, long key, int level)
        {
            // 一个协程队列一帧处理超过100个,说明比较多了,打个warning,检查一下是否够正常
            if (level == 100)
            {
                Log.Warning($"too much coroutine level: {coroutineLockType} {key}");
            }

            this.nextFrameRun.Enqueue((coroutineLockType, key, level));
        }

        private void AddTimer(long tillTime, CoroutineLock coroutineLock)
        {
            this.timers.Add(tillTime, new CoroutineLockTimer(coroutineLock));
            if (tillTime < this.minTime)
            {
                this.minTime = tillTime;
            }
        }

        public async ETTask<CoroutineLock> Wait(int coroutineLockType, long key, int time = 60000)
        {
            CoroutineLockQueueType coroutineLockQueueType = this.list[coroutineLockType];

            if (!coroutineLockQueueType.TryGetValue(key, out CoroutineLockQueue queue))
            {
                coroutineLockQueueType.Add(key, CoroutineLockQueue.Create());
                return this.CreateCoroutineLock(coroutineLockType, key, time, 1);
            }

            ETTask<CoroutineLock> tcs = ETTask<CoroutineLock>.Create(true);
            queue.Add(tcs, time);
            return await tcs;
        }

        private CoroutineLock CreateCoroutineLock(int coroutineLockType, long key, int time, int level)
        {
            CoroutineLock coroutineLock = CoroutineLock.Create(coroutineLockType, key, level);
            if (time > 0)
            {
                this.AddTimer(TimeHelper.ClientFrameTime() + time, coroutineLock);
            }

            return coroutineLock;
        }

        public void Notify(int coroutineLockType, long key, int level)
        {
            CoroutineLockQueueType coroutineLockQueueType = this.list[coroutineLockType];
            if (!coroutineLockQueueType.TryGetValue(key, out CoroutineLockQueue queue))
            {
                return;
            }

            if (queue.Count == 0)
            {
                coroutineLockQueueType.Remove(key);
                return;
            }

            CoroutineLockInfo coroutineLockInfo = queue.Dequeue();
            coroutineLockInfo.Tcs.SetResult(this.CreateCoroutineLock(coroutineLockType, key, coroutineLockInfo.Time, level));
        }
    }
}