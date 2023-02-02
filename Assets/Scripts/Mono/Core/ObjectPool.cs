using System;
using System.Collections.Generic;

namespace TaoTie
{
    public class ObjectPool: IDisposable
    {
        private readonly Dictionary<Type, Queue<object>> pool = new Dictionary<Type, Queue<object>>();
        
        public static ObjectPool Instance = new ObjectPool();
        
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
            return queue.Dequeue() as T;
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
            return queue.Dequeue();
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
        }

        public void Dispose()
        {
            this.pool.Clear();
        }
    }
}