using System.Collections.Generic;

namespace TaoTie
{
    public class UnOrderDoubleKeyMap<T, M, N>: Dictionary<T, UnOrderMultiMap<M, N>>
    {
        // 重用HashSet
        public new List<N> this[T t,M m]
        {
            get
            {
                List<N> set;
                if (!this.TryGetValue(t, out var map))
                {
                    map = new UnOrderMultiMap<M, N>();
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
                map = new UnOrderMultiMap<M, N>();
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
                foreach (KeyValuePair<T,UnOrderMultiMap<M, N>> kv in this)
                {
                    count += kv.Value.Count;
                }
                return count;
            }
        }
    }
}