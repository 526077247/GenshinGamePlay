using System;
using System.Collections.Generic;
using System.Reflection;

namespace TaoTie
{

    public class Messager : IManager
    {
        public static Messager Instance;

        public void Init()
        {
            Instance = this;
        }

        public void Destroy()
        {
            evtGroup.Clear();
            Instance = null;
        }

        readonly Dictionary<long, MultiMapSet<int, MulticastDelegate>> evtGroup = new();

        #region 注册

        public void AddListener(long id, int name, Action evt)
        {
            if (!evtGroup.ContainsKey(id))
            {
                evtGroup.Add(id, new MultiMapSet<int, MulticastDelegate>());
            }

            evtGroup[id].Add(name, evt);
        }

        public void AddListener<P1>(long id, int name, Action<P1> evt)
        {
            if (!evtGroup.ContainsKey(id))
            {
                evtGroup.Add(id, new MultiMapSet<int, MulticastDelegate>());
            }

            evtGroup[id].Add(name, evt);
        }

        public void AddListener<P1, P2>(long id, int name, Action<P1, P2> evt)
        {
            if (!evtGroup.ContainsKey(id))
            {
                evtGroup.Add(id, new MultiMapSet<int, MulticastDelegate>());
            }

            evtGroup[id].Add(name, evt);
        }

        public void AddListener<P1, P2, P3>(long id, int name, Action<P1, P2, P3> evt)
        {
            if (!evtGroup.ContainsKey(id))
            {
                evtGroup.Add(id, new MultiMapSet<int, MulticastDelegate>());
            }

            evtGroup[id].Add(name, evt);
        }

        public void AddListener<P1, P2, P3, P4>(long id, int name, Action<P1, P2, P3, P4> evt)
        {
            if (!evtGroup.ContainsKey(id))
            {
                evtGroup.Add(id, new MultiMapSet<int, MulticastDelegate>());
            }

            evtGroup[id].Add(name, evt);
        }

        public void AddListener<P1, P2, P3, P4, P5>(long id, int name, Action<P1, P2, P3, P4, P5> evt)
        {
            if (!evtGroup.ContainsKey(id))
            {
                evtGroup.Add(id, new MultiMapSet<int, MulticastDelegate>());
            }

            evtGroup[id].Add(name, evt);
        }

        #endregion

        #region 取消注册

        public void RemoveListener(long id, int name, Action evt)
        {
            if (evtGroup.TryGetValue(id, out var evts))
            {
                evts.Remove(name, evt);
            }
        }

        public void RemoveListener<P1>(long id, int name, Action<P1> evt)
        {
            if (evtGroup.TryGetValue(id, out var evts))
            {
                evts.Remove(name, evt);
            }
        }

        public void RemoveListener<P1, P2>(long id, int name, Action<P1, P2> evt)
        {
            if (evtGroup.TryGetValue(id, out var evts))
            {
                evts.Remove(name, evt);
            }
        }

        public void RemoveListener<P1, P2, P3>(long id, int name, Action<P1, P2, P3> evt)
        {
            if (evtGroup.TryGetValue(id, out var evts))
            {
                evts.Remove(name, evt);
            }
        }

        public void RemoveListener<P1, P2, P3, P4>(long id, int name, Action<P1, P2, P3, P4> evt)
        {
            if (evtGroup.TryGetValue(id, out var evts))
            {
                evts.Remove(name, evt);
            }
        }

        public void RemoveListener<P1, P2, P3, P4, P5>(long id, int name, Action<P1, P2, P3, P4, P5> evt)
        {
            if (evtGroup.TryGetValue(id, out var evts))
            {
                evts.Remove(name, evt);
            }
        }

        #endregion

        #region 广播

        public void Broadcast(long id, int name)
        {
            if (evtGroup.TryGetValue(id, out var evts))
            {
                if (evts.TryGetValue(name, out var evt))
                {
                    using var list = ToList(evt);
                    for (int i = 0; i < list.Count; i++)
                    {
                        var item = list[i];
                        (item as Action)?.Invoke();
                    }
                }
            }
        }

        public void Broadcast<P1>(long id, int name, P1 p1)
        {
            if (evtGroup.TryGetValue(id, out var evts))
            {
                if (evts.TryGetValue(name, out var evt))
                {
                    using var list = ToList(evt);
                    for (int i = 0; i < list.Count; i++)
                    {
                        var item = list[i];
                        if (item is Action<P1> action)
                        {
                            action.Invoke(p1);
                        }
                        else //多态支持
                        {
                            var param = item.GetMethodInfo().GetParameters();
                            if (param.Length == 1 && param[0].ParameterType.IsAssignableFrom(TypeInfo<P1>.Type))
                                item.DynamicInvoke(p1);
                        }
                    }
                }
            }
        }

        public void Broadcast<P1, P2>(long id, int name, P1 p1, P2 p2)
        {
            if (evtGroup.TryGetValue(id, out var evts))
            {
                if (evts.TryGetValue(name, out var evt))
                {
                    using var list = ToList(evt);
                    for (int i = 0; i < list.Count; i++)
                    {
                        var item = list[i];
                        (item as Action<P1, P2>)?.Invoke(p1, p2);
                    }
                }
            }
        }

        public void Broadcast<P1, P2, P3>(long id, int name, P1 p1, P2 p2, P3 p3)
        {
            if (evtGroup.TryGetValue(id, out var evts))
            {
                if (evts.TryGetValue(name, out var evt))
                {
                    using var list = ToList(evt);
                    for (int i = 0; i < list.Count; i++)
                    {
                        var item = list[i];
                        (item as Action<P1, P2, P3>)?.Invoke(p1, p2, p3);
                    }
                }
            }
        }

        public void Broadcast<P1, P2, P3, P4>(long id, int name, P1 p1, P2 p2, P3 p3, P4 p4)
        {
            if (evtGroup.TryGetValue(id, out var evts))
            {
                if (evts.TryGetValue(name, out var evt))
                {
                    using var list = ToList(evt);
                    for (int i = 0; i < list.Count; i++)
                    {
                        var item = list[i];
                        (item as Action<P1, P2, P3, P4>)?.Invoke(p1, p2, p3, p4);
                    }
                }
            }
        }

        public void Broadcast<P1, P2, P3, P4, P5>(long id, int name, P1 p1, P2 p2, P3 p3, P4 p4, P5 p5)
        {
            if (evtGroup.TryGetValue(id, out var evts))
            {
                if (evts.TryGetValue(name, out var evt))
                {
                    using var list = ToList(evt);
                    for (int i = 0; i < list.Count; i++)
                    {
                        var item = list[i];
                        (item as Action<P1, P2, P3, P4, P5>)?.Invoke(p1, p2, p3, p4, p5);
                    }
                }
            }
        }

        #endregion

        #region 下一帧广播

        public async ETTask BroadcastNextFrame(long id, int name)
        {
            if (evtGroup.TryGetValue(id, out var evts))
            {
                if (evts.TryGetValue(name, out var evt))
                {
                    await TimerManager.Instance.WaitAsync(1);

                    using var list = ToList(evt);
                    for (int i = 0; i < list.Count; i++)
                    {
                        var item = list[i];
                        (item as Action)?.Invoke();
                    }
                }
            }
        }

        public async ETTask BroadcastNextFrame<P1>(long id, int name, P1 p1)
        {
            if (evtGroup.TryGetValue(id, out var evts))
            {
                if (evts.TryGetValue(name, out var evt))
                {
                    await TimerManager.Instance.WaitAsync(1);
                    using var list = ToList(evt);
                    for (int i = 0; i < list.Count; i++)
                    {
                        var item = list[i];
                        if (item is Action<P1> action)
                        {
                            action.Invoke(p1);
                        }
                        else //多态支持
                        {
                            var param = item.GetMethodInfo().GetParameters();
                            if (param.Length == 1 && param[0].ParameterType.IsAssignableFrom(TypeInfo<P1>.Type))
                                item.DynamicInvoke(p1);
                        }
                    }
                }
            }
        }

        public async ETTask BroadcastNextFrame<P1, P2>(long id, int name, P1 p1, P2 p2)
        {
            if (evtGroup.TryGetValue(id, out var evts))
            {
                if (evts.TryGetValue(name, out var evt))
                {
                    await TimerManager.Instance.WaitAsync(1);
                    using var list = ToList(evt);
                    for (int i = 0; i < list.Count; i++)
                    {
                        var item = list[i];
                        (item as Action<P1, P2>)?.Invoke(p1, p2);
                    }
                }
            }
        }

        public async ETTask BroadcastNextFrame<P1, P2, P3>(long id, int name, P1 p1, P2 p2, P3 p3)
        {
            if (evtGroup.TryGetValue(id, out var evts))
            {
                if (evts.TryGetValue(name, out var evt))
                {
                    await TimerManager.Instance.WaitAsync(1);
                    using var list = ToList(evt);
                    for (int i = 0; i < list.Count; i++)
                    {
                        var item = list[i];
                        (item as Action<P1, P2, P3>)?.Invoke(p1, p2, p3);
                    }
                }
            }
        }

        public async ETTask BroadcastNextFrame<P1, P2, P3, P4>(long id, int name, P1 p1, P2 p2, P3 p3, P4 p4)
        {
            if (evtGroup.TryGetValue(id, out var evts))
            {
                if (evts.TryGetValue(name, out var evt))
                {
                    await TimerManager.Instance.WaitAsync(1);
                    using var list = ToList(evt);
                    for (int i = 0; i < list.Count; i++)
                    {
                        var item = list[i];
                        (item as Action<P1, P2, P3, P4>)?.Invoke(p1, p2, p3, p4);
                    }
                }
            }
        }

        public async ETTask BroadcastNextFrame<P1, P2, P3, P4, P5>(long id, int name, P1 p1, P2 p2, P3 p3, P4 p4, P5 p5)
        {
            if (evtGroup.TryGetValue(id, out var evts))
            {
                if (evts.TryGetValue(name, out var evt))
                {
                    await TimerManager.Instance.WaitAsync(1);
                    using var list = ToList(evt);
                    for (int i = 0; i < list.Count; i++)
                    {
                        var item = list[i];
                        (item as Action<P1, P2, P3, P4, P5>)?.Invoke(p1, p2, p3, p4, p5);
                    }
                }
            }
        }

        #endregion

        public ListComponent<MulticastDelegate> ToList(HashSet<MulticastDelegate> set)
        {
            ListComponent<MulticastDelegate> res = ListComponent<MulticastDelegate>.Create();
            if (set != null)
            {
                foreach (var item in set)
                {
                    res.Add(item);
                }
            }

            return res;
        }
    }
}