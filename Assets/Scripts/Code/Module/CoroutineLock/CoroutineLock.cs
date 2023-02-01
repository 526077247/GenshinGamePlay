using System;

namespace TaoTie
{
    public class CoroutineLock:IDisposable
    {
        public int coroutineLockType;
        public long key;
        public int level;
        public long InstanceId;
        public static CoroutineLock Create(int type, long k, int count)
        {
            var res = ObjectPool.Instance.Fetch(TypeInfo<CoroutineLock>.Type) as CoroutineLock;
            res.coroutineLockType = type;
            res.key = k;
            res.level = count;
            res.InstanceId = IdGenerater.Instance.GenerateInstanceId();
            return res;
        }

        public void Dispose()
        {
            if (this.coroutineLockType != CoroutineLockType.None)
            {
                CoroutineLockManager.Instance.RunNextCoroutine(this.coroutineLockType, this.key, this.level + 1);
            }
            else
            {
                // CoroutineLockType.None说明协程锁超时了
                Log.Error($"coroutine lock timeout: {this.coroutineLockType} {this.key} {this.level}");
            }
            this.coroutineLockType = CoroutineLockType.None;
            this.key = 0;
            this.level = 0;
            ObjectPool.Instance.Recycle(this);
        }
    }
}