using System;
using System.Collections.Generic;

namespace TaoTie
{
    public class AttributeManager:IManager
    {
        public static AttributeManager Instance { get; private set; }
        
        private readonly UnOrderMultiMap<Type, Type> types = new UnOrderMultiMap<Type, Type>();
        
        private readonly List<Type> Empty = new List<Type>();
        
        #region override

        public void Init()
        {
            Instance = this;
            this.types.Clear();
            HashSet<Type> temp = new HashSet<Type>();
            var allTypes = AssemblyManager.Instance.GetTypes();
            foreach (var item in allTypes)
            {
                Type type = item.Value;

                if (type.IsAbstract)
                {
                    continue;
                }
                
                // 记录所有的有BaseAttribute标记的的类型
                object[] objects = type.GetCustomAttributes(typeof(BaseAttribute), true);
                temp.Clear();
                foreach (object o in objects)
                {
                    var otype = o.GetType();
                    if(temp.Contains(otype)) continue;
                    this.types.Add(otype, type);
                    temp.Add(otype);
                }
            }

        }

        public void Destroy()
        {
            Instance = null;
            types.Clear();
        }

        #endregion
        
        public List<Type> GetTypes(Type systemAttributeType)
        {
            if (this.types.TryGetValue(systemAttributeType, out var res))
                return res;
            return Empty;
        }
        
    }
}