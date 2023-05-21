using System.Collections;
using System.Collections.Generic;

namespace TaoTie
{
    public class PriorityStack<T> :IEnumerable<T> where T: IPriorityStackItem
    {
        protected UnOrderMultiMap<int, T> priorityStacks;
        protected LinkedList<int> priorityList;
        protected int count;
        public int Count => count;

        public PriorityStack()
        {
            priorityStacks = new UnOrderMultiMap<int, T>();
            priorityList = new LinkedList<int>();
        }

        public T this[int index]
        {
            get
            {
                foreach (var item in priorityList)
                {
                    var list = priorityStacks[item];
                    if (index >= list.Count)
                    {
                        index -= list.Count;
                    }
                    else
                    {
                        return list[list.Count - 1 - index];
                    }
                }
                return default;
            }
        }


        public void Push(T item)
        {
            bool add = false;
            for (var node = priorityList.First; node!=null; node = node.Next)
            {
                if (node.Value == item.Priority)
                {
                    add = true;
                    break;
                }

                if (node.Value < item.Priority)
                {
                    priorityList.AddBefore(node, item.Priority);
                    add = true;
                    break;
                }
            }

            if (!add)
            {
                priorityList.AddLast(item.Priority);
            }
            priorityStacks.Add(item.Priority, item);
            count++;
        }

        public T Pop(int index = 0)
        {
            var res = this[index];
            if (priorityStacks.Remove(res.Priority, res))
            {
                if (priorityStacks[res.Priority]==null||priorityStacks[res.Priority].Count == 0)
                {
                    priorityList.Remove(res.Priority);
                }
                count--;
            }
            return res;
        }

        public T Peek(int index = 0)
        {
            return this[index];
        }

        public T Remove(T res)
        {
            if (priorityStacks.Remove(res.Priority, res))
            {
                if (priorityStacks[res.Priority]==null||priorityStacks[res.Priority].Count == 0)
                {
                    priorityList.Remove(res.Priority);
                }
                count--;
            }
            return res;
        }
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (var kv in priorityStacks)
            {
                if(kv.Value==null) continue;
                foreach (var item in kv.Value)
                {
                    yield return item;
                }
            }
        }
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            foreach (var kv in priorityStacks)
            {
                if(kv.Value==null) continue;
                foreach (var item in kv.Value)
                {
                    yield return item;
                }
            }
        }
    }
}