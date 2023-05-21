using System;
using System.Collections.Generic;

namespace TaoTie
{
    public class ManagerProvider
    {
        private ManagerProvider()
        {
            allManagers = new LinkedList<object>();
            managersDictionary = new UnOrderDoubleKeyDictionary<Type,string, object>();
            updateManagers = new LinkedList<IUpdate>();
        }
        static ManagerProvider Instance { get; } = new ManagerProvider();
        
        UnOrderDoubleKeyDictionary<Type,string, object> managersDictionary;
        private LinkedList<object> allManagers;
        private LinkedList<IUpdate> updateManagers;
        public static T GetManager<T>(string name = "") where T :class,IManagerDestroy
        {
            var type = TypeInfo<T>.Type;
            if (!Instance.managersDictionary.TryGetValue(type, name, out var res))
            {
                return null;
            }
            return res as T;
        }
        public static T RegisterManager<T>(string name = "") where T :class,IManager
        {
            var type = TypeInfo<T>.Type;
            if (!Instance.managersDictionary.TryGetValue(type, name, out var res))
            {
                res = Activator.CreateInstance(type) as T;
                if (res is IUpdate u)
                {
                    Instance.updateManagers.AddLast(u);
                }
                (res as T).Init();
                Instance.managersDictionary.Add(type,name,res);
                Instance.allManagers.AddLast(res);
            }
            return res as T;
        }
        public static T RegisterManager<T,P1>(P1 p1,string name = "") where T :class,IManager<P1>
        {
            var type = TypeInfo<T>.Type;
            if (!Instance.managersDictionary.TryGetValue(type, name, out var res))
            {
                res = Activator.CreateInstance(type) as T;
                if (res is IUpdate u)
                {
                    Instance.updateManagers.AddLast(u);
                }
                (res as T).Init(p1);
                Instance.managersDictionary.Add(type,name,res);
                Instance.allManagers.AddLast(res);
            }
            return res as T;
        }
        public static T RegisterManager<T,P1,P2>(P1 p1,P2 p2,string name = "") where T :class,IManager<P1,P2>
        {
            var type = TypeInfo<T>.Type;
            if (!Instance.managersDictionary.TryGetValue(type, name, out var res))
            {
                res = Activator.CreateInstance(type) as T;
                if (res is IUpdate u)
                {
                    Instance.updateManagers.AddLast(u);
                }
                (res as T).Init(p1,p2);
                Instance.managersDictionary.Add(type,name,res);
                Instance.allManagers.AddLast(res);
            }
            return res as T;
        }
        public static T RegisterManager<T,P1,P2,P3>(P1 p1,P2 p2,P3 p3,string name = "") where T :class,IManager<P1,P2,P3>
        {
            var type = TypeInfo<T>.Type;
            if (!Instance.managersDictionary.TryGetValue(type, name, out var res))
            {
                res = Activator.CreateInstance(type) as T;
                if (res is IUpdate u)
                {
                    Instance.updateManagers.AddLast(u);
                }
                (res as T).Init(p1,p2,p3);
                Instance.managersDictionary.Add(type,name,res);
                Instance.allManagers.AddLast(res);
            }
            return res as T;
        }

        public static void RemoveManager<T>(string name = "")
        {
            var type = TypeInfo<T>.Type;
            if (Instance.managersDictionary.TryGetValue(type, name, out var res))
            {
                if (res is IUpdate u)
                {
                    Instance.updateManagers.Remove(u);
                }
                Instance.managersDictionary.Remove(type,name);
                Instance.allManagers.Remove(res);
                (res as IManagerDestroy)?.Destroy();
            }
        }

        public static void Clear()
        {
            Instance.managersDictionary.Clear();
            Instance.updateManagers.Clear();
            foreach (var item in Instance.allManagers)
            {
                (item as IManagerDestroy)?.Destroy();
            }
            Instance.allManagers.Clear();
        }
        public static void Update()
        {
            for (var node = Instance.updateManagers.First; node !=null; node=node.Next)
            {
                node.Value.Update();
            }
            int count = frameFinishTask.Count;
            while (count-- > 0)
            {
                ETTask task = frameFinishTask.Dequeue();
                task.SetResult();
            }
        }
        
        private static Queue<ETTask> frameFinishTask = new Queue<ETTask>();
        //等待这一帧所有update结束
        public static ETTask WaitFrameFinish()
        {
            ETTask task = ETTask.Create(true);
            frameFinishTask.Enqueue(task);
            return task;
        }
    }
}