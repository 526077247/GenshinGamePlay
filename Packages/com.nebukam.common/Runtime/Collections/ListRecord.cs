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
using Nebukam;

namespace Nebukam.Collections
{

    public class ListRecord<TItem>
        where TItem : class
    {

        protected List<TItem> m_list;
        protected Dictionary<TItem, int> m_punch;

        public int Count { get { return m_list.Count; } }
        public List<TItem> list { get { return m_list; } }
        public TItem this[int i] { get { return m_list[i]; } }
        public int this[TItem item]
        {
            get
            {
                int amount;
                if (m_punch.TryGetValue(item, out amount))
                    return amount;

                return -1;
            }
        }

        public ListRecord()
        {
            m_list = new List<TItem>(10);
            m_punch = new Dictionary<TItem, int>(10);
        }

        public ListRecord(int initialCapacity)
        {
            m_list = new List<TItem>(initialCapacity);
            m_punch = new Dictionary<TItem, int>(initialCapacity);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public int Add(TItem item)
        {
            if (m_list.TryAddOnce(item))
            {
                m_punch[item] = 1;
                return 1;
            }

            int value = m_punch[item] + 1;
            m_punch[item] = value;
            return value;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns>-1 if the item was not present, otherwise remainder</returns>
        public int Remove(TItem item)
        {
            int amount;
            if (m_punch.TryGetValue(item, out amount))
            {
                amount--;

                if (amount <= 0)
                {
                    m_list.Remove(item);
                    m_punch.Remove(item);
                    return 0;
                }
                else
                {
                    m_punch[item] = amount;
                }
            }

            return -1;
        }

        public int PunchCount(TItem item)
        {
            int amount;

            if (m_punch.TryGetValue(item, out amount))
                return amount;

            return -1;

        }

        public void Clear()
        {
            m_list.Clear();
            m_punch.Clear();
        }

    }

}
