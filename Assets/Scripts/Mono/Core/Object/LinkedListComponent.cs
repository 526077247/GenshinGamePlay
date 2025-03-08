using System;
using System.Collections.Generic;

namespace TaoTie
{
    public class LinkedListComponent<T>: LinkedList<T>, IDisposable
    {
        public static LinkedListComponent<T> Create()
        {
            return ObjectPool.Instance.Fetch<LinkedListComponent<T>>();
        }

        public void Dispose()
        {
            this.Clear();
            ObjectPool.Instance.Recycle(this);
        }
    }
}