using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using UnityEngine.UI;

namespace TaoTie
{ 
    public class LruCache<TKey, TValue>:IEnumerable<KeyValuePair<TKey, TValue>>
    {
        const int DEFAULT_CAPACITY = 255;

        int capacity;
        ReaderWriterLockSlim locker;
        Dictionary<TKey, TValue> dictionary;
        LinkedList<TKey> linkedList;
        Func<TKey, TValue, bool> checkCanPopFunc;
        Action<TKey, TValue> popCb;
        public LruCache() : this(DEFAULT_CAPACITY) { }

        public LruCache(int capacity)
        {
            locker = new ReaderWriterLockSlim();
            this.capacity = capacity > 0 ? capacity : DEFAULT_CAPACITY;
            dictionary = new Dictionary<TKey, TValue>(DEFAULT_CAPACITY);
            linkedList = new LinkedList<TKey>();
        }

        public void SetCheckCanPopCallback(Func<TKey,TValue, bool> func)
        {
            checkCanPopFunc = func;
        }

        public void SetPopCallback(Action<TKey, TValue> func)
        {
            popCb = func;
        }

        public TValue this[TKey t]
        {
            get
            {
                if(TryGet(t, out var res))
                    return res;
                throw new ArgumentException();
            }
            set
            {
                Set(t, value);
            }
        }

        public void Set(TKey key, TValue value)
        {
            locker.EnterWriteLock();
            try
            {
                if(checkCanPopFunc!=null)
                    MakeFreeSpace();
                dictionary[key] = value;
                linkedList.Remove(key);
                linkedList.AddFirst(key);
                if (checkCanPopFunc==null&&linkedList.Count > capacity)
                {
                    dictionary.Remove(linkedList.Last.Value);
                    linkedList.RemoveLast();
                }
            }
            finally { locker.ExitWriteLock(); }
        }
        public Dictionary<TKey, TValue> GetAll()
        {
            return dictionary as Dictionary<TKey, TValue>;
        }
        public void Remove(TKey key)
        {
            locker.EnterWriteLock();
            try
            {
                dictionary.Remove(key);
                linkedList.Remove(key);
            }
            finally { locker.ExitWriteLock(); }
        }

        public bool TryOnlyGet(TKey key, out TValue value)
        {
            bool b = dictionary.TryGetValue(key, out value);
            return b;
        }
        public bool TryGet(TKey key, out TValue value)
        {
            locker.EnterUpgradeableReadLock();
            try
            {
                bool b = dictionary.TryGetValue(key, out value);
                if (b)
                {
                    locker.EnterWriteLock();
                    try
                    {
                        linkedList.Remove(key);
                        linkedList.AddFirst(key);
                    }
                    finally { locker.ExitWriteLock(); }
                }
                return b;
            }
            catch { throw; }
            finally { locker.ExitUpgradeableReadLock(); }
        }

        public bool ContainsKey(TKey key)
        {
            locker.EnterReadLock();
            try
            {
                return dictionary.ContainsKey(key);
            }
            finally { locker.ExitReadLock(); }
        }

        public int Count
        {
            get
            {
                locker.EnterReadLock();
                try
                {
                    return dictionary.Count;
                }
                finally { locker.ExitReadLock(); }
            }
        }

        public int Capacity
        {
            get
            {
                locker.EnterReadLock();
                try
                {
                    return capacity;
                }
                finally { locker.ExitReadLock(); }
            }
            set
            {
                locker.EnterUpgradeableReadLock();
                try
                {
                    if (value > 0 && capacity != value)
                    {
                        locker.EnterWriteLock();
                        try
                        {
                            capacity = value;
                            while (linkedList.Count > capacity)
                            {
                                linkedList.RemoveLast();
                            }
                        }
                        finally { locker.ExitWriteLock(); }
                    }
                }
                finally { locker.ExitUpgradeableReadLock(); }
            }
        }

        public ICollection<TKey> Keys
        {
            get
            {
                locker.EnterReadLock();
                try
                {
                    return dictionary.Keys;
                }
                finally { locker.ExitReadLock(); }
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                locker.EnterReadLock();
                try
                {
                    return dictionary.Values;
                }
                finally { locker.ExitReadLock(); }
            }
        }
        //remotes elements to provide enough memory
        //returns last removed element or nil
        void MakeFreeSpace() 
        {
            var key = linkedList.Last;

            var max_check_free_times = 10;// max check free times for avoid no tuple can free cause iterator much times;
            var curCheckFreeTime = 0;


            while(linkedList.Count + 1 > DEFAULT_CAPACITY){
                 if (key==null) break;

                var tuple_prev = key.Previous;
                if (checkCanPopFunc == null || checkCanPopFunc(key.Value, dictionary[key.Value]))
                {
                    //can pop
                    var value = dictionary[key.Value];
                    dictionary.Remove(key.Value);
                    linkedList.Remove(key.Value);
                    popCb?.Invoke(key.Value, value);
                }
                else
                {
                    //the host say cannot pop
                    curCheckFreeTime ++;
                    if (curCheckFreeTime > max_check_free_times)
                    {
                        //lru cache detect check_free time is too much, please check code
                        break;
                    }
                }
                key = tuple_prev;
            }

        }
        public void CleanUp() 
        {
            var key = linkedList.Last;
            int count = linkedList.Count;
            while(count > 0)
            {
                count--;
                var tuple_prev = key.Previous;
                if (checkCanPopFunc == null || checkCanPopFunc(key.Value, dictionary[key.Value]))
                {
                    //can pop
                    var value = dictionary[key.Value];
                    dictionary.Remove(key.Value);
                    linkedList.Remove(key.Value);
                    popCb?.Invoke(key.Value, value);
                }
                key = tuple_prev;
            }
        }
        public void Clear()
        {
            dictionary.Clear();
            linkedList.Clear();
        }
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (var item in dictionary)
            {
                yield return item;
            }
        }
        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            foreach (var item in dictionary)
            {
                yield return item;
            }
        }
    }

}