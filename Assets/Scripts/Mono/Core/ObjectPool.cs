using System;
using System.Collections.Generic;

namespace TaoTie
{
    public class ObjectPool: IDisposable
    {
        private readonly Dictionary<Type, Queue<object>> pool = new Dictionary<Type, Queue<object>>();
        
        public static ObjectPool Instance = new ObjectPool();

#if UNITY_EDITOR
        private HashSet<object> poolCheck = new HashSet<object>();
#endif
        private ObjectPool()
        {
        }
        public T Fetch<T>() where T :class
        {
            Type type = TypeInfo<T>.Type;
            Queue<object> queue = null;
            if (!pool.TryGetValue(type, out queue))
            {
                return Activator.CreateInstance(type) as T;
            }

            if (queue.Count == 0)
            {
                return Activator.CreateInstance(type) as T;
            }
            var res = queue.Dequeue();
#if UNITY_EDITOR
            if (!poolCheck.Contains(res))
            {
                Log.Error("对象池重复取"+res);
            }
            poolCheck.Remove(res);
#endif
            return res as T;
        }
        public object Fetch(Type type)
        {
            Queue<object> queue = null;
            if (!pool.TryGetValue(type, out queue))
            {
                return Activator.CreateInstance(type);
            }

            if (queue.Count == 0)
            {
                return Activator.CreateInstance(type);
            }
            var res = queue.Dequeue();
#if UNITY_EDITOR
            if (!poolCheck.Contains(res))
            {
                Log.Error("对象池重复取"+res);
            }
            poolCheck.Remove(res);
#endif
            return res;
        }

        public void Recycle(object obj)
        {
            Type type = obj.GetType();
            Queue<object> queue = null;
            if (!pool.TryGetValue(type, out queue))
            {
                queue = new Queue<object>();
                pool.Add(type, queue);
            }
            queue.Enqueue(obj);
#if UNITY_EDITOR
            if (poolCheck.Contains(obj))
            {
                Log.Error("对象池重复回收"+obj);
            }
            poolCheck.Add(obj);
#endif
        }

        public void Dispose()
        {
            this.pool.Clear();
        }
    }
}