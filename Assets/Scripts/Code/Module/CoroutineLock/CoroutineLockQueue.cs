using System.Collections.Generic;
using System;
namespace TaoTie
{
    public struct CoroutineLockInfo
    {
        public ETTask<CoroutineLock> Tcs;
        public int Time;
    }

    public class CoroutineLockQueue:IDisposable
    {
        public Queue<CoroutineLockInfo> queue = new Queue<CoroutineLockInfo>();

        public int Count
        {
            get
            {
                return this.queue.Count;
            }
        }

        public static CoroutineLockQueue Create()
        {
            return ObjectPool.Instance.Fetch(typeof (CoroutineLockQueue)) as CoroutineLockQueue;
        }

        public void Dispose()
        {
            queue.Clear();
            ObjectPool.Instance.Recycle(this);
        }

        public void Add(ETTask<CoroutineLock> tcs, int time)
        {
            this.queue.Enqueue(new CoroutineLockInfo(){Tcs = tcs, Time = time});
        }
        
        public CoroutineLockInfo Dequeue()
        {
            return this.queue.Dequeue();
        }
    }
}