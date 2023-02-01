using System;

namespace TaoTie
{
    public sealed class Variable<T> : IVariable, IDisposable
    {
        public T value;

        public static Variable<T> Create()
        {
            return ObjectPool.Instance.Fetch<Variable<T>>();
        }

        public void Dispose()
        {
            value = default;
            ObjectPool.Instance.Recycle(this);
        }

        public Type GetValueType()
        {
            return this.GetType();
        }
        public string debugValue => value.ToString();
    }
}
