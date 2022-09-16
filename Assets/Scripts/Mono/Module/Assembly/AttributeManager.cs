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
            List<Type> baseAttributeTypes = AssemblyManager.Instance.GetBaseAttributes();
            foreach (Type baseAttributeType in baseAttributeTypes)
            {
                foreach (var kv in AssemblyManager.Instance.GetTypes())
                {
                    Type type = kv.Value;
                    if (type.IsAbstract)
                    {
                        continue;
                    }

                    object[] objects = type.GetCustomAttributes(baseAttributeType, true);
                    if (objects.Length == 0)
                    {
                        continue;
                    }

                    this.types.Add(baseAttributeType, type);
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