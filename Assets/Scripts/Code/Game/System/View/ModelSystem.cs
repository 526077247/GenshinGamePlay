using System;
using System.Collections.Generic;

namespace TaoTie
{
    public class ModelSystem: IManager,IUpdate
    {
        public static ModelSystem Instance;
        private Dictionary<Type, Type> configArrangeType;
        private LinkedList<ArrangePlugin> arrangePlugins;

        public void Init()
        {
            Instance = this;
            arrangePlugins = new LinkedList<ArrangePlugin>();
            configArrangeType = new Dictionary<Type, Type>();
            var allTypes = AssemblyManager.Instance.GetTypes();
            var runnerType = TypeInfo<ArrangePlugin>.Type;
            foreach (var item in allTypes)
            {
                var type = item.Value;
                if (!type.IsAbstract && runnerType.IsAssignableFrom(type))
                {
                    configArrangeType.Add(type.BaseType.GenericTypeArguments[0],type);
                }
            }
        }

        public void Destroy()
        {
            arrangePlugins = null;
            configArrangeType = null;
            Instance = null;
        }

        public void Update()
        {
            for (var node = arrangePlugins.First; node !=null; )
            {
                var next = node.Next;
                if (node.Value.IsDispose)
                {
                    arrangePlugins.Remove(node);
                }
                else
                {
                    node.Value.Update();
                }
                node = next;
            }
        }
        
        /// <summary>
        /// 创建ArrangePlugin
        /// </summary>
        /// <param name="config"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public ArrangePlugin CreateArrangePlugin<T>(T config,UnitModelComponent unitModelComponent) where T:ConfigArrange
        {
            if(configArrangeType.TryGetValue(config.GetType(), out var type))
            {
                var res = ObjectPool.Instance.Fetch(type) as ArrangePlugin;
                res.Init(config,unitModelComponent);
                arrangePlugins.AddLast(res);
                return res;
            }
            Log.Error($"CreateArrangePlugin失败， 未实现{config.GetType()}对应的ArrangePlugin");
            return null;
        }
    }
}