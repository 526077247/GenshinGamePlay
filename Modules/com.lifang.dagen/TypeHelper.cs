using System;
using System.Collections.Generic;
using System.Reflection;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif
namespace DaGenGraph
{
    public static class TypeHelper
    {
        /// <summary>
        /// Cached Models
        /// </summary>
        private static readonly Dictionary<string, Type> TypeCodes = new Dictionary<string, Type>(30)
        {
            {typeof(byte).FullName, typeof(byte)},
            {typeof(sbyte).FullName, typeof(sbyte)},
            {typeof(short).FullName, typeof(short)},
            {typeof(ushort).FullName, typeof(ushort)},
            {typeof(int).FullName, typeof(int)},
            {typeof(uint).FullName, typeof(uint)},
            {typeof(long).FullName, typeof(long)},
            {typeof(ulong).FullName, typeof(ulong)},
            {typeof(float).FullName, typeof(float)},
            {typeof(double).FullName, typeof(double)},
            {typeof(decimal).FullName, typeof(decimal)},
            {typeof(char).FullName, typeof(char)},
            {typeof(bool).FullName, typeof(bool)},
            {typeof(string).FullName, typeof(string)},
            {typeof(DateTime).FullName, typeof(DateTime)}
        };
        private static Dictionary<Type, List<Type>> subTypes = new Dictionary<Type, List<Type>>();
        private static Dictionary<Type, string[]> fullNames = new Dictionary<Type, string[]>();
        private static Dictionary<string, Type> tempType = new Dictionary<string, Type>();
        private static HashSet<string> noneFind = new HashSet<string>();
        public static List<Type> GetSubClassList(FieldInfo fieldInfo, Type type, out string[] names)
        {
            if (type == null)
            {
                names = null;
                return null;
            }
            fullNames.TryGetValue(type, out names);
            if (subTypes.TryGetValue(type, out var res)) return res;
            res = new List<Type>();
            if (!type.IsAbstract)
            {
                res.Add(type);
            }
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; i++)
            {
                var types = assemblies[i].GetTypes();
                foreach (var item in types)
                {
                    if (item.IsClass && !item.IsAbstract && type.IsAssignableFrom(item))
                    {
                        res.Add(item);   
                    }
                }
            }

            names = new string[res.Count];
            for (int i = 0; i < names.Length; i++)
            {
                if (res[i].GetCustomAttribute(typeof(LabelTextAttribute)) is LabelTextAttribute labelTextAttribute)
                {
                    names[i] = labelTextAttribute.Text;
                }
                else
                {
                    names[i] = res[i].FullName;
                }
            }
            fullNames.Add(type, names);
            subTypes.Add(type,res);
            return res;
        }
        public static Type FindType(string name)
        {
            if(TypeCodes.TryGetValue(name,out var type))
            {
                return type;
            }
            if (tempType.TryGetValue(name, out type))
            {
                return type;
            }
            if (noneFind.Contains(name))
            {
                return null;
            }
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; i++)
            {
                var types = assemblies[i].GetTypes();
                foreach (var item in types)
                {
                    if (item.IsClass)
                    {
                        if (item.FullName == name)
                        {
                            tempType.Add(name,item);
                            return item;
                        }
                        if (item.Name == name)
                        {
                            type = item;
                        }
                    }
                }
            }

            if (type != null)
            {
                tempType.Add(name,type);
            }
            else
            {
                noneFind.Add(name);
            }
            return type;
        }
    }
}