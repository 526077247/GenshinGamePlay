using System.Collections.Generic;
using System;
namespace TaoTie
{
    public class CoroutineLockQueueType:IDisposable
    {
        private Dictionary<long, CoroutineLockQueue> dictionary;

        public static CoroutineLockQueueType Create()
        {
            var res = ObjectPool.Instance.Fetch(TypeInfo<CoroutineLockQueueType>.Type) as CoroutineLockQueueType;
            res.dictionary = new Dictionary<long, CoroutineLockQueue>();
            return res;
        }

        public void Dispose()
        {
            this.dictionary.Clear();
            ObjectPool.Instance.Recycle(this);
        }

        public bool TryGetValue(long key, out CoroutineLockQueue value)
        {
            return this.dictionary.TryGetValue(key, out value);
        }

        public void Remove(long key)
        {
            if (this.dictionary.TryGetValue(key, out CoroutineLockQueue value))
            {
                value.Dispose();
            }
            this.dictionary.Remove(key);
        }
        
        public void Add(long key, CoroutineLockQueue value)
        {
            this.dictionary.Add(key, value);
        }
    }
}