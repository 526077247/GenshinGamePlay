using System;
using System.Collections.Generic;

namespace TaoTie
{
    public class ManagerProvider
    {
        private ManagerProvider()
        {
            AllManagers = new LinkedList<object>();
            _managersDictionary = new UnOrderDoubleKeyDictionary<Type,string, object>();
            UpdateManagers = new LinkedList<IUpdateManager>();
        }
        static ManagerProvider Instance { get; } = new ManagerProvider();
        
        UnOrderDoubleKeyDictionary<Type,string, object> _managersDictionary;
        private LinkedList<object> AllManagers;
        private LinkedList<IUpdateManager> UpdateManagers;
        public static T GetManager<T>(string name = "") where T :class,IManagerDestroy
        {
            var type = typeof(T);
            if (!Instance._managersDictionary.TryGetValue(type, name, out var res))
            {
                return null;
            }
            return res as T;
        }
        public static T RegisterManager<T>(string name = "") where T :class,IManager
        {
            var type = typeof(T);
            if (!Instance._managersDictionary.TryGetValue(type, name, out var res))
            {
                res = Activator.CreateInstance(type) as T;
                if (res is IUpdateManager u)
                {
                    Instance.UpdateManagers.AddLast(u);
                }

                (res as T).Init();
                Instance._managersDictionary.Add(type,name,res);
                Instance.AllManagers.AddLast(res);
            }
            return res as T;
        }
        public static T RegisterManager<T,P1>(P1 p1,string name = "") where T :class,IManager<P1>
        {
            var type = typeof(T);
            if (!Instance._managersDictionary.TryGetValue(type, name, out var res))
            {
                res = Activator.CreateInstance(type) as T;
                if (res is IUpdateManager u)
                {
                    Instance.UpdateManagers.AddLast(u);
                }
                (res as T).Init(p1);
                Instance._managersDictionary.Add(type,name,res);
                Instance.AllManagers.AddLast(res);
            }
            return res as T;
        }
        public static T RegisterManager<T,P1,P2>(P1 p1,P2 p2,string name = "") where T :class,IManager<P1,P2>
        {
            var type = typeof(T);
            if (!Instance._managersDictionary.TryGetValue(type, name, out var res))
            {
                res = Activator.CreateInstance(type) as T;
                if (res is IUpdateManager u)
                {
                    Instance.UpdateManagers.AddLast(u);
                }
                (res as T).Init(p1,p2);
                Instance._managersDictionary.Add(type,name,res);
                Instance.AllManagers.AddLast(res);
            }
            return res as T;
        }
        public static T RegisterManager<T,P1,P2,P3>(P1 p1,P2 p2,P3 p3,string name = "") where T :class,IManager<P1,P2,P3>
        {
            var type = typeof(T);
            if (!Instance._managersDictionary.TryGetValue(type, name, out var res))
            {
                res = Activator.CreateInstance(type) as T;
                if (res is IUpdateManager u)
                {
                    Instance.UpdateManagers.AddLast(u);
                }
                (res as T).Init(p1,p2,p3);
                Instance._managersDictionary.Add(type,name,res);
                Instance.AllManagers.AddLast(res);
            }
            return res as T;
        }

        public static void RemoveManager<T>(string name = "")
        {
            var type = typeof(T);
            if (Instance._managersDictionary.TryGetValue(type, name, out var res))
            {
                if (res is IUpdateManager u)
                {
                    Instance.UpdateManagers.Remove(u);
                }
                
                Instance._managersDictionary.Remove(type,name);
                Instance.AllManagers.Remove(res);
                (res as IManagerDestroy)?.Destroy();
            }
        }

        public static void Clear()
        {
            Instance._managersDictionary.Clear();
            Instance.UpdateManagers.Clear();
            
            foreach (var item in Instance.AllManagers)
            {
                (item as IManagerDestroy)?.Destroy();
            }
            Instance.AllManagers.Clear();
        }
        public static void Update()
        {
            for (var node = Instance.UpdateManagers.First; node !=null; node=node.Next)
            {
                node.Value.Update();
            }
        }
    }
}