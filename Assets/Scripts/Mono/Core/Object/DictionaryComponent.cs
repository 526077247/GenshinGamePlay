using System;
using System.Collections.Generic;

namespace TaoTie
{
    public class DictionaryComponent<T,V>: Dictionary<T,V>, IDisposable
    {
        public static DictionaryComponent<T,V> Create()
        {
            return ObjectPool.Instance.Fetch<DictionaryComponent<T,V>>();
        }

        public void Dispose()
        {
            this.Clear();
            ObjectPool.Instance.Recycle(this);
        }
    }
}