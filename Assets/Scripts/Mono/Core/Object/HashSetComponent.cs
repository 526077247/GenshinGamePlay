using System;
using System.Collections.Generic;

namespace TaoTie
{
    public class HashSetComponent<T>: HashSet<T>, IDisposable
    {
        public static HashSetComponent<T> Create()
        {
            return ObjectPool.Instance.Fetch(TypeInfo<HashSetComponent<T>>.Type) as HashSetComponent<T>;
        }

        public void Dispose()
        {
            this.Clear();
            ObjectPool.Instance.Recycle(this);
        }
    }
}