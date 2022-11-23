using System;
using System.Collections.Generic;

namespace TaoTie
{

    public class Messager:IManager
    {
        public static Messager Instance => ManagerProvider.RegisterManager<Messager>();
        public void Init()
        {
            
        }

        public void Destroy()
        {
            evts.Clear();
        }
        
        readonly Dictionary<string, HashSet<MulticastDelegate>> evts = new Dictionary<string, HashSet<MulticastDelegate>>();

        #region 注册

        public void AddListener(string name, Action evt)
        {
            if (!evts.ContainsKey(name))
                evts.Add(name, new HashSet<MulticastDelegate>());
            evts[name].Add(evt);
        }
        public void AddListener<P1>(string name, Action<P1> evt)
        {
            if (!evts.ContainsKey(name))
                evts.Add(name, new HashSet<MulticastDelegate>());
            evts[name].Add(evt);
        }
        public void AddListener<P1,P2>(string name, Action<P1,P2> evt)
        {
            if (!evts.ContainsKey(name))
                evts.Add(name, new HashSet<MulticastDelegate>());
            evts[name].Add(evt);
        }
        public void AddListener<P1,P2,P3>(string name, Action<P1,P2,P3> evt)
        {
            if (!evts.ContainsKey(name))
                evts.Add(name, new HashSet<MulticastDelegate>());
            evts[name].Add(evt);
        }
        public void AddListener<P1,P2,P3,P4>(string name, Action<P1,P2,P3,P4> evt)
        {
            if (!evts.ContainsKey(name))
                evts.Add(name, new HashSet<MulticastDelegate>());
            evts[name].Add(evt);
        }
        public void AddListener<P1,P2,P3,P4,P5>(string name, Action<P1,P2,P3,P4,P5> evt)
        {
            if (!evts.ContainsKey(name))
                evts.Add(name, new HashSet<MulticastDelegate>());
            evts[name].Add(evt);
        }

        #endregion

        #region 取消注册
        
        public void RemoveListener(string name, Action evt)
        {
            if (evts.ContainsKey(name))
            {
                evts[name].Remove(evt);
            }
        }
        public void RemoveListener<P1>(string name, Action<P1> evt)
        {
            if (evts.ContainsKey(name))
            {
                evts[name].Remove(evt);
            }
        }
        public void RemoveListener<P1,P2>(string name, Action<P1,P2> evt)
        {
            if (evts.ContainsKey(name))
            {
                evts[name].Remove(evt);
            }
        }
        public void RemoveListener<P1,P2,P3>(string name, Action<P1,P2,P3> evt)
        {
            if (evts.ContainsKey(name))
            {
                evts[name].Remove(evt);
            }
        }
        public void RemoveListener<P1,P2,P3,P4>(string name, Action<P1,P2,P3,P4> evt)
        {
            if (evts.ContainsKey(name))
            {
                evts[name].Remove(evt);
            }
        }
        public void RemoveListener<P1,P2,P3,P4,P5>(string name, Action<P1,P2,P3,P4,P5> evt)
        {
            if (evts.ContainsKey(name))
            {
                evts[name].Remove(evt);
            }
        }
        
        #endregion

        #region 广播
        
        public void Broadcast(string name)
        {
            if (evts.TryGetValue(name, out var evt))
            {
                foreach (var item in evt)
                {
                    (item as Action)?.Invoke();
                }
            }
        }
        
        public void Broadcast<P1>(string name, P1 p1)
        {
            if (evts.TryGetValue(name, out var evt))
            {
                foreach (var item in evt)
                {
                    (item as Action<P1>)?.Invoke(p1);
                }
            }
        }
        
        public void Broadcast<P1,P2>(string name, P1 p1,P2 p2)
        {
            if (evts.TryGetValue(name, out var evt))
            {
                foreach (var item in evt)
                {
                    (item as Action<P1,P2>)?.Invoke(p1,p2);
                }
            }
        }
        
        public void Broadcast<P1,P2,P3>(string name, P1 p1,P2 p2,P3 p3)
        {
            if (evts.TryGetValue(name, out var evt))
            {
                foreach (var item in evt)
                {
                    (item as Action<P1,P2,P3>)?.Invoke(p1,p2,p3);
                }
            }
        }
        
        public void Broadcast<P1,P2,P3,P4>(string name, P1 p1,P2 p2,P3 p3,P4 p4)
        {
            if (evts.TryGetValue(name, out var evt))
            {
                foreach (var item in evt)
                {
                    (item as Action<P1,P2,P3,P4>)?.Invoke(p1,p2,p3,p4);
                }
            }
        }
        
        public void Broadcast<P1,P2,P3,P4,P5>(string name, P1 p1,P2 p2,P3 p3,P4 p4,P5 p5)
        {
            if (evts.TryGetValue(name, out var evt))
            {
                foreach (var item in evt)
                {
                    (item as Action<P1,P2,P3,P4,P5>)?.Invoke(p1,p2,p3,p4,p5);
                }
            }
        }
        
        #endregion
        
        #region 下一帧广播
        
        public async ETTask BroadcastNextFrame(string name)
        {
            await TimerManager.Instance.WaitAsync(1);
            if (evts.TryGetValue(name, out var evt))
            {
                foreach (var item in evt)
                {
                    (item as Action)?.Invoke();
                }
            }
        }
        
        public async ETTask BroadcastNextFrame<P1>(string name, P1 p1)
        {
            await TimerManager.Instance.WaitAsync(1);
            if (evts.TryGetValue(name, out var evt))
            {
                foreach (var item in evt)
                {
                    (item as Action<P1>)?.Invoke(p1);
                }
            }
        }
        
        public async ETTask BroadcastNextFrame<P1,P2>(string name, P1 p1,P2 p2)
        {
            await TimerManager.Instance.WaitAsync(1);
            if (evts.TryGetValue(name, out var evt))
            {
                foreach (var item in evt)
                {
                    (item as Action<P1,P2>)?.Invoke(p1,p2);
                }
            }
        }
        
        public async ETTask BroadcastNextFrame<P1,P2,P3>(string name, P1 p1,P2 p2,P3 p3)
        {
            await TimerManager.Instance.WaitAsync(1);
            if (evts.TryGetValue(name, out var evt))
            {
                foreach (var item in evt)
                {
                    (item as Action<P1,P2,P3>)?.Invoke(p1,p2,p3);
                }
            }
        }
        
        public async ETTask BroadcastNextFrame<P1,P2,P3,P4>(string name, P1 p1,P2 p2,P3 p3,P4 p4)
        {
            await TimerManager.Instance.WaitAsync(1);
            if (evts.TryGetValue(name, out var evt))
            {
                foreach (var item in evt)
                {
                    (item as Action<P1,P2,P3,P4>)?.Invoke(p1,p2,p3,p4);
                }
            }
        }
        
        public async ETTask BroadcastNextFrame<P1,P2,P3,P4,P5>(string name, P1 p1,P2 p2,P3 p3,P4 p4,P5 p5)
        {
            await TimerManager.Instance.WaitAsync(1);
            if (evts.TryGetValue(name, out var evt))
            {
                foreach (var item in evt)
                {
                    (item as Action<P1,P2,P3,P4,P5>)?.Invoke(p1,p2,p3,p4,p5);
                }
            }
        }
        
        #endregion
    }
}