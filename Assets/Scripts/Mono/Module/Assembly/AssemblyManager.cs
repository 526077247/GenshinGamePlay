using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace TaoTie
{
    public class AssemblyManager:IManager
    {
        public static AssemblyManager Instance;
        private HashSet<Assembly> temp;
        private HashSet<Assembly> hotfixTemp;
        private Dictionary<string, Type> allTypes;
        private UnOrderMultiMap<Assembly, Type> mapTypes;
        #region override

        public void Init()
        {
            Instance = this;
            temp = new HashSet<Assembly>();
            hotfixTemp = new HashSet<Assembly>();
            allTypes = new Dictionary<string, Type>();
            mapTypes = new UnOrderMultiMap<Assembly, Type>();
        }

        public void Destroy()
        {
            Instance = null;
            temp.Clear();
            hotfixTemp.Clear();
            allTypes.Clear();
            mapTypes.Clear();
        }

        #endregion

        public Dictionary<string, Type> GetTypes()
        {
            return allTypes;
        }

        public void AddAssembly(Assembly assembly)
        {
            if (!temp.Contains(assembly))
            {
                foreach (Type type in assembly.GetTypes())
                {
                    allTypes[type.FullName] = type;
                    mapTypes.Add(assembly,type);
                }
            }
        }
        
        public void AddHotfixAssembly(Assembly assembly)
        {
            hotfixTemp.Add(assembly);
            AddAssembly(assembly);
        }

        public void RemoveHotfixAssembly()
        {
            foreach (var assembly in hotfixTemp)
            {
                if (mapTypes.TryGetValue(assembly, out var types))
                {
                    foreach (var type in types)
                    {
                        allTypes.Remove(type.FullName);
                    }
                }

                mapTypes.Remove(assembly);
                temp.Remove(assembly);
            }
            hotfixTemp.Clear();
        }
    }
}