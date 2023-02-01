using System;

namespace TaoTie
{
    internal interface IVariable
    {
        public void Dispose();
        public string debugValue { get; }

        public Type GetValueType();
    }
}
