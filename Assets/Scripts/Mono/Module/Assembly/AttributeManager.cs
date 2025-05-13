using System;
using System.Collections.Generic;

namespace TaoTie
{
    public class AttributeManager:IManager
    {
        public static AttributeManager Instance { get; private set; }
        
        private readonly UnOrderMultiMap<Type, Type> types = new UnOrderMultiMap<Type, Type>();
        
        private readonly List<Type> Empty = new List<Type>();
        
        private UnOrderDoubleKeyDictionary<Type, Type, Type> creatableTypeMap;
        
        #region override

        public void Init()
        {
            Instance = this;
            creatableTypeMap = new UnOrderDoubleKeyDictionary<Type, Type, Type>();
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
                object[] objects = type.GetCustomAttributes(TypeInfo<BaseAttribute>.Type, true);
                temp.Clear();
                foreach (object o in objects)
                {
                    var otype = o.GetType();
                    if(temp.Contains(otype)) continue;
                    this.types.Add(otype, type);
                    temp.Add(otype);
                }
            }

            var creatableTypes = types.GetAll(TypeInfo<CreatableAttribute>.Type);
          
            foreach (var item in allTypes)
            {
                var type = item.Value;
                for (int i = 0; i < creatableTypes.Length; i++)
                {
                    var pluginType = creatableTypes[i];
                    if (!type.IsAbstract && pluginType.IsAssignableFrom(type))
                    {
                        creatableTypeMap.Add(pluginType, type.BaseType.GenericTypeArguments[0], type);
                    }
                }
            }
        }

        public void Destroy()
        {
            Instance = null;
            types.Clear();
            creatableTypeMap = null;
        }

        #endregion
        
        public List<Type> GetTypes(Type systemAttributeType)
        {
            if (this.types.TryGetValue(systemAttributeType, out var res))
                return res;
            return Empty;
        }

        public Dictionary<Type, Type> GetCreateTypeMap(Type baseType)
        {
            if (creatableTypeMap.TryGetDic(baseType, out var res))
            {
                return res;
            }
            res = new Dictionary<Type, Type>();
            var allTypes = AssemblyManager.Instance.GetTypes();
            foreach (var item in allTypes)
            {
                var type = item.Value;
                if (!type.IsAbstract && baseType.IsAssignableFrom(type))
                {
                    res.Add(type.BaseType.GenericTypeArguments[0],type);
                }
            }
            creatableTypeMap.Add(baseType, res);
            return res;
        }
    }
}