using System;

namespace TaoTie
{
    public static class TypeInfo<T>
    {
        public static readonly Type Type = typeof(T);

        public static readonly int HashCode = typeof(T).GetHashCode();

        public static readonly string TypeName = typeof(T).Name;
    }
}