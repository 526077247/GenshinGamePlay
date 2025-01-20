using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DaGenGraph
{
    public static class TypeHelper
    {
        private static Dictionary<Type, List<Type>> subTypes = new Dictionary<Type, List<Type>>();
        private static Dictionary<Type, string[]> fullNames = new Dictionary<Type, string[]>();
        public static List<Type> GetSubClassList(Type type,out string[] names)
        {
            if (type == null)
            {
                names = null;
                return null;
            }
            fullNames.TryGetValue(type, out names);
            if (subTypes.TryGetValue(type, out var res)) return res;
            res = new List<Type>();

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
                names[i] = res[i].FullName;
            }
            fullNames.Add(type, names);
            subTypes.Add(type,res);
            return res;
        }
    }
}