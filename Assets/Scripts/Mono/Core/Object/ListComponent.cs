using System;
using System.Collections.Generic;

namespace TaoTie
{

    public class ListComponent<T>: List<T>, IDisposable
    {
        public static ListComponent<T> Create()
        {
            return ObjectPool.Instance.Fetch<ListComponent<T>>();
        }

        public void Dispose()
        {
            this.Clear();
            ObjectPool.Instance.Recycle(this);
        }
    }
}