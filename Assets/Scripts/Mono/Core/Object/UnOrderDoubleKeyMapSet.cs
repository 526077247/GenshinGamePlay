using System.Collections.Generic;

namespace TaoTie
{
    public class UnOrderDoubleKeyMapSet<T, M, N>: Dictionary<T, UnOrderMultiMapSet<M, N>>
    {
        // 重用HashSet
        public new HashSet<N> this[T t,M m]
        {
            get
            {
                HashSet<N> set;
                if (!this.TryGetValue(t, out var map))
                {
                    map = new UnOrderMultiMapSet<M, N>();
                    return map[m];
                }
                return map[m];
            }
        }

        public void Add(T t, M m, N k)
        {
            this.TryGetValue(t, out var map);
            if (map == null)
            {
                map = new UnOrderMultiMapSet<M, N>();
                base[t] = map;
            }
            map.Add(m, k);
        }

        public bool Remove(T t, M m, N k)
        {
            this.TryGetValue(t, out var map);
            if (map == null)
            {
                return false;
            }
            return map.Remove(m, k);
        }

        public bool Contains(T t, M m, N k)
        {
            this.TryGetValue(t, out var map);
            if (map == null)
            {
                return false;
            }
            return map.Contains(m, k);
        }

        public new int Count
        {
            get
            {
                int count = 0;
                foreach (KeyValuePair<T,UnOrderMultiMapSet<M, N>> kv in this)
                {
                    count += kv.Value.Count;
                }
                return count;
            }
        }
    }
}