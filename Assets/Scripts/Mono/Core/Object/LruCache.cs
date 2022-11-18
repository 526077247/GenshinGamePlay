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

        int _capacity;
        ReaderWriterLockSlim _locker;
        Dictionary<TKey, TValue> _dictionary;
        LinkedList<TKey> _linkedList;
        Func<TKey, TValue, bool> check_can_pop_func;
        Action<TKey, TValue> pop_cb;
        public LruCache() : this(DEFAULT_CAPACITY) { }

        public LruCache(int capacity)
        {
            _locker = new ReaderWriterLockSlim();
            _capacity = capacity > 0 ? capacity : DEFAULT_CAPACITY;
            _dictionary = new Dictionary<TKey, TValue>(DEFAULT_CAPACITY);
            _linkedList = new LinkedList<TKey>();
        }

        public void SetCheckCanPopCallback(Func<TKey,TValue, bool> func)
        {
            check_can_pop_func = func;
        }

        public void SetPopCallback(Action<TKey, TValue> func)
        {
            pop_cb = func;
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
            _locker.EnterWriteLock();
            try
            {
                if(check_can_pop_func!=null)
                    __MakeFreeSpace();
                _dictionary[key] = value;
                _linkedList.Remove(key);
                _linkedList.AddFirst(key);
                if (check_can_pop_func==null&&_linkedList.Count > _capacity)
                {
                    _dictionary.Remove(_linkedList.Last.Value);
                    _linkedList.RemoveLast();
                }
            }
            finally { _locker.ExitWriteLock(); }
        }
        public Dictionary<TKey, TValue> GetAll()
        {
            return _dictionary as Dictionary<TKey, TValue>;
        }
        public void Remove(TKey key)
        {
            _locker.EnterWriteLock();
            try
            {
                _dictionary.Remove(key);
                _linkedList.Remove(key);
            }
            finally { _locker.ExitWriteLock(); }
        }

        public bool TryOnlyGet(TKey key, out TValue value)
        {
            bool b = _dictionary.TryGetValue(key, out value);
            return b;
        }
        public bool TryGet(TKey key, out TValue value)
        {
            _locker.EnterUpgradeableReadLock();
            try
            {
                bool b = _dictionary.TryGetValue(key, out value);
                if (b)
                {
                    _locker.EnterWriteLock();
                    try
                    {
                        _linkedList.Remove(key);
                        _linkedList.AddFirst(key);
                    }
                    finally { _locker.ExitWriteLock(); }
                }
                return b;
            }
            catch { throw; }
            finally { _locker.ExitUpgradeableReadLock(); }
        }

        public bool ContainsKey(TKey key)
        {
            _locker.EnterReadLock();
            try
            {
                return _dictionary.ContainsKey(key);
            }
            finally { _locker.ExitReadLock(); }
        }

        public int Count
        {
            get
            {
                _locker.EnterReadLock();
                try
                {
                    return _dictionary.Count;
                }
                finally { _locker.ExitReadLock(); }
            }
        }

        public int Capacity
        {
            get
            {
                _locker.EnterReadLock();
                try
                {
                    return _capacity;
                }
                finally { _locker.ExitReadLock(); }
            }
            set
            {
                _locker.EnterUpgradeableReadLock();
                try
                {
                    if (value > 0 && _capacity != value)
                    {
                        _locker.EnterWriteLock();
                        try
                        {
                            _capacity = value;
                            while (_linkedList.Count > _capacity)
                            {
                                _linkedList.RemoveLast();
                            }
                        }
                        finally { _locker.ExitWriteLock(); }
                    }
                }
                finally { _locker.ExitUpgradeableReadLock(); }
            }
        }

        public ICollection<TKey> Keys
        {
            get
            {
                _locker.EnterReadLock();
                try
                {
                    return _dictionary.Keys;
                }
                finally { _locker.ExitReadLock(); }
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                _locker.EnterReadLock();
                try
                {
                    return _dictionary.Values;
                }
                finally { _locker.ExitReadLock(); }
            }
        }
        //remotes elements to provide enough memory
        //returns last removed element or nil
        void __MakeFreeSpace() {
            var key = _linkedList.Last;

            var max_check_free_times = 10;// max check free times for avoid no tuple can free cause iterator much times;
            var cur_check_free_time = 0;


            while(_linkedList.Count + 1 > DEFAULT_CAPACITY){
                 if (key==null) break;

                var tuple_prev = key.Previous;
                if (check_can_pop_func == null || check_can_pop_func(key.Value, _dictionary[key.Value]))
                {
                    //can pop
                    Remove(key.Value);
                    pop_cb?.Invoke(key.Value, _dictionary[key.Value]);
                }
                else
                {
                    //the host say cannot pop
                    cur_check_free_time = cur_check_free_time + 1;
                    if (cur_check_free_time > max_check_free_times)
                    {
                        //lru cache detect check_free time is too much, please check code
                        break;
                    }
                }
                key = tuple_prev;
            }

        }
        
        public void Clear()
        {
            _dictionary.Clear();
            _linkedList.Clear();
        }
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (var item in _dictionary)
            {
                yield return item;
            }
        }
        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            foreach (var item in _dictionary)
            {
                yield return item;
            }
        }
    }

}