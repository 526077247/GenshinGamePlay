using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using DaGenGraph;
using DaGenGraph.Editor;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace TaoTie
{
    public class OdinNodeView<T>: NodeView<T> where T: NodeBase
    {
        private static Dictionary<FieldInfo,string[]> valueDropdown = new Dictionary<FieldInfo, string[]>();
        private static List<string> temp = new List<string>();
        private static Dictionary<string, Type> tempType = new Dictionary<string, Type>();
        protected override float DrawFieldInspector(FieldInfo field, object obj, bool isDetails = false)
        {
            if (field.FieldType == typeof(string) && 
                field.GetCustomAttribute(typeof(ValueDropdownAttribute)) is ValueDropdownAttribute valueDropdownAttribute)
            {
                Type type = obj.GetType();
                string funcName = valueDropdownAttribute.ValuesGetter;
                if (valueDropdownAttribute.ValuesGetter.StartsWith("@"))
                {
                    if (valueDropdownAttribute.ValuesGetter.Contains("."))
                    {
                        var vs = valueDropdownAttribute.ValuesGetter.Split(".");
                        type = FinType(vs[0].Replace("@",""));
                        funcName = vs[1];
                    }
                    if (funcName.Contains("("))
                    {
                        if (funcName.Contains("()"))
                        {
                            funcName = funcName.Replace("()", "");
                        }
                        else
                        {
                            //todo:先不处理
                            type = null;
                        }
                    }
                }
                if (type != null)
                {
                    var method = type.GetMethod(funcName, BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static);
                    if (method != null)
                    {
                        if (!valueDropdown.TryGetValue(field,out var list))
                        {
                            IEnumerable data;
                            if (method.IsStatic)
                            {
                                data = method.Invoke(null,null) as IEnumerable;
                            }
                            else
                            {
                                data = method.Invoke(obj,null) as IEnumerable;
                            }
                            temp.Clear();
                            foreach (var item in data)
                            {
                                temp.Add(item.ToString());
                            }
                            list = temp.ToArray();
                            valueDropdown.Add(field,list);
                        }
                        object value = field.GetValue(obj);
                        int index = -1;
                        for (int i = 0; i < list.Length; i++)
                        {
                            if (value == list[i])
                            {
                                index = i;
                                break;
                            }
                        }
                        GUILayout.BeginHorizontal();
                        GUILayout.Label(field.Name, GUILayout.Width(150));
                        var newindex = EditorGUILayout.Popup(index, list);
                        if (newindex != index)
                        {
                            field.SetValue(obj, EditorGUILayout.TextField(field.Name, list[newindex],GUILayout.ExpandWidth(true)));
                        }
                        GUILayout.EndHorizontal();
                        return 21;
                    }
                    
                }
                
            }
            
            return base.DrawFieldInspector(field, obj, isDetails);
        }
        
        protected Type FinType(string name)
        {
            if (tempType.TryGetValue(name, out var type))
            {
                return type;
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
                            tempType.Add(name,type);
                            return type;
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
            return type;
        }
    }
}