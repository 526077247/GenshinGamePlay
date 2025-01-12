using System;
using System.Collections.Generic;
using Nino.Core;

namespace TaoTie
{
    [NinoIgnore]
    public class ListComponent<T>: List<T>, IDisposable
    {
        public static ListComponent<T> Create()
        {
            return ObjectPool.Instance.Fetch(TypeInfo<ListComponent<T>>.Type) as ListComponent<T>;
        }

        public void Dispose()
        {
            this.Clear();
            ObjectPool.Instance.Recycle(this);
        }
    }
}