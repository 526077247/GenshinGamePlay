// Copyright (c) 2021 Timothé Lapetite - nebukam@gmail.com.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System.Collections.Generic;
using Nebukam.Collections;

namespace Nebukam.Collections
{
    public class ListDictionary<TKey, TValue> : PoolItem, IRequireCleanUp
    {

        protected List<TKey> m_keyList = new List<TKey>(10);
        protected Dictionary<TKey, List<TValue>> m_dictionary = new Dictionary<TKey, List<TValue>>(10);
        
        public int KeyCount { get { return m_dictionary.Count; } }
        public List<TKey> keyList { get { return m_keyList; } }
        public List<TValue> this[TKey key] { get { return m_dictionary[key]; } }

        public int ValueCount(TKey key)
        {
            
            if (m_dictionary.TryGetValue(key, out List<TValue> entries))
                return entries.Count;

            return 0;
        }

        public bool TryGet(TKey key, out List<TValue> values)
        {
            if (m_dictionary.TryGetValue(key, out values))
                return true;

            values = null;
            return false;
        }

        public bool Contains(TKey key)
        {
            return m_dictionary.ContainsKey(key);
        }

        public bool Contains(TKey key, TValue value)
        {
            if (m_dictionary.TryGetValue(key, out List<TValue> values))
                if (values.IndexOf(value) != -1)
                    return true;

            return false;
        }

        public TValue Add(TKey key, TValue value)
        {
            if (!m_dictionary.ContainsKey(key)) 
            { 
                m_keyList.AddOnce(key);
                m_dictionary[key] = RentValueList(value);
            }
            else
            {
                m_dictionary[key].AddOnce(value);
            }

            return value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns>True if the value has been added, false if it was already present</returns>
        public bool TryAdd(TKey key, TValue value)
        {

            if (!m_dictionary.ContainsKey(key))
            {
                m_keyList.AddOnce(key);
                m_dictionary[key] = RentValueList(value);
            }
            else
            {
                return m_dictionary[key].TryAddOnce(value);
            }

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns>True if the value has been removed, false if it was not present</returns>
        public bool Remove(TKey key, TValue value)
        {
            if (m_dictionary.TryGetValue(key, out List<TValue> entries))
            {
                if (entries.TryRemove(value))
                {
                    if (entries.Count == 0)
                    {
                        ReturnValueList(entries);
                        m_dictionary.Remove(key);
                        m_keyList.Remove(key);
                    }

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns>True if the key list has been removed, false if it was not present</returns>
        public bool Remove(TKey key)
        {
            if (m_dictionary.TryGetValue(key, out List<TValue> entries))
            {
                entries.Clear();
                ReturnValueList(entries);
                m_dictionary.Remove(key);
                m_keyList.Remove(key);
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Clear()
        {
            int count = m_keyList.Count;
            for (int i = 0; i < count; i++)
            {
                List<TValue> list = m_dictionary[m_keyList[i]];
                list.Clear();
                ReturnValueList(list);
            }

            m_keyList.Clear();
            m_dictionary.Clear();
        }

        public void CleanUp()
        {
            Clear();
        }

        #region internal list pool

        internal static List<List<TValue>> m_valueListPool = null;

        private static List<TValue> RentValueList(TValue firstValue)
        {
            if (m_valueListPool == null || m_valueListPool.Count == 0)
                return new List<TValue>(10) { firstValue };

            List<TValue> list = m_valueListPool.Pop();
            list.Add(firstValue);
            return list;
        }

        private static void ReturnValueList(List<TValue> list)
        {
            if (m_valueListPool == null)
                m_valueListPool = new List<List<TValue>>(10);

            m_valueListPool.AddOnce(list);
        }

        #endregion

    }
}
