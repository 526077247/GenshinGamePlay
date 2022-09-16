using System;
using System.Collections.Generic;

namespace TaoTie
{
    /// <summary>
    /// 注意：装箱极其严重，慎用！！！！
    /// </summary>
    public class Messager:IManager
    {
        public static Messager Instance => ManagerProvider.GetManager<Messager>();
        public void Init()
        {
            
        }

        public void Destroy()
        {
            evts.Clear();
        }


        readonly Dictionary<string, HashSet<MulticastDelegate>> evts = new Dictionary<string, HashSet<MulticastDelegate>>();

        public void AddListener<T>(string name, T evt) where T : MulticastDelegate
        {
            if (!evts.ContainsKey(name))
                evts.Add(name, new HashSet<MulticastDelegate>());
            evts[name].Add(evt);
        }

        public void RemoveListener<T>(string name, T evt) where T : MulticastDelegate
        {
            if (evts.ContainsKey(name))
            {
                evts[name].Remove(evt);
            }
        }

        public void Broadcast<T>(string name, params object[] param) where T : MulticastDelegate
        {
            if (evts.TryGetValue(name, out var evt))
            {
                foreach (var item in evt)
                {
                    (item as T)?.DynamicInvoke(param);
                }
            }
        }
    }
}