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
    public abstract class OdinNodeView<T>: NodeView<T> where T: NodeBase
    {
        private static Dictionary<FieldInfo,string[]> valueDropdown = new Dictionary<FieldInfo, string[]>();
        private static List<string> temp = new List<string>();
        private static Dictionary<string, Type> tempType = new Dictionary<string, Type>();
        private static Dictionary<Type, string[]> enumDropDown = new Dictionary<Type, string[]>();
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
                        string value = field.GetValue(obj) as string;
                        int index = -1;
                        for (int i = 0; i < list.Length; i++)
                        {
                            if (value == list[i])
                            {
                                index = i;
                                break;
                            }
                        }
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(GetShowName(field,out _), GUILayout.Width(100));
                        var newindex = EditorGUILayout.Popup(index, list);
                        if (newindex != index)
                        {
                            field.SetValue(obj, list[newindex]);
                        }
                        EditorGUILayout.EndHorizontal();
                        return 21;
                    }
                    
                }
                
            }

            if (field.FieldType.IsEnum)
            {
                if (!enumDropDown.TryGetValue(field.FieldType, out var names))
                {
                    names = Enum.GetNames(field.FieldType);
                    bool has = false;
                    for (int i = 0; i < names.Length; i++)
                    {
                        var enumField = field.FieldType.GetField(names[i]);
                        names[i] = GetShowName(enumField,out bool rename);
                        has |= rename;
                    }
                    if (!has)
                    {
                        names = null;
                    }
                    enumDropDown.Add(field.FieldType,names);
                }
                
                if (names == null)
                {
                    var value = field.GetValue(obj);
                    field.SetValue(obj, EditorGUILayout.EnumPopup(GetShowName(field,out _), (Enum) value));
                }
                else
                {
                    var value = field.GetValue(obj);
                    int index = -1;
                    var list = Enum.GetValues(field.FieldType);
                    for (int i = 0; i < list.Length; i++)
                    {
                        if (value.Equals(list.GetValue(i)))
                        {
                            index = i;
                            break;
                        }
                    }
                    
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(GetShowName(field,out _), GUILayout.Width(100));
                    var newindex = EditorGUILayout.Popup(index, names);
                    if (newindex != index)
                    {
                        field.SetValue(obj, list.GetValue(newindex));
                    }
                    EditorGUILayout.EndHorizontal();
                }
                return 21;
            }
            return base.DrawFieldInspector(field, obj, isDetails);
        }
        
        protected virtual Type FinType(string name)
        {
            if (name == nameof(OdinDropdownHelper)) return typeof(OdinDropdownHelper);
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

        protected override string GetShowName(FieldInfo field,out bool rename)
        {
            if (field.GetCustomAttribute(typeof(LabelTextAttribute)) is LabelTextAttribute labelTextAttribute)
            {
                rename = true;
                return labelTextAttribute.Text;
            }
            return base.GetShowName(field,out rename);
        }
    }
}