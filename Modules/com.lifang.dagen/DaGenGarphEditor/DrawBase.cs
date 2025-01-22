using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace DaGenGraph.Editor
{
    public abstract class DrawBase: EditorWindow
    {
        private static Type stringType = typeof(string);
        private static Type listType = typeof(List<>);
        private static Type dicType = typeof(Dictionary<,>);
        private HashSet<FieldInfo> foldoutState = new HashSet<FieldInfo>();
        private Dictionary<FieldInfo,HashSet<int>> listFoldoutState = new Dictionary<FieldInfo,HashSet<int>>();
        private Dictionary<FieldInfo, object> dicInputKey = new();
        protected virtual float DrawObjectInspector(object obj, bool isDetails = false)
        {
            float height = 0;
            if (obj == null) return height;
            var fields = obj.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);

            foreach (var field in fields)
            {
                var attribute = field.GetCustomAttribute(typeof(DrawIgnoreAttribute));
                if (attribute is DrawIgnoreAttribute ignoreAttribute)
                {
                    if (ignoreAttribute.Ignore == Ignore.All) continue;
                    if (ignoreAttribute.Ignore == Ignore.Details == isDetails) continue;
                }
                if (field.GetCustomAttribute(typeof(HideInInspector)) is HideInInspector)
                {
                    continue;
                }
                
                if (field.GetCustomAttribute(typeof(TooltipAttribute)) is TooltipAttribute tooltip)
                {
                    EditorGUILayout.HelpBox(tooltip.tooltip,MessageType.Info);
                    height += 40;
                }
                if (field.GetCustomAttribute(typeof(HeaderAttribute)) is HeaderAttribute header)
                {
                    EditorGUILayout.LabelField(header.header);
                    height += 20;
                }
                if (field.GetCustomAttribute(typeof(SpaceAttribute)) is SpaceAttribute space)
                {
                    EditorGUILayout.Space(space.height);
                    height += space.height;
                }
                height += DrawFieldInspector(field, obj, isDetails);
            }

            return height;
        }

        protected virtual float DrawFieldInspector(FieldInfo field, object obj, bool isDetails = false)
        {
            object value = field.GetValue(obj);
            object newValue = value;
            // 显示字段名称和对应的值
            var height = ShowNormalField(field.FieldType,GetShowName(field,out _), ref newValue, isDetails,field);
            if (height > 0)
            {
                if (newValue != value)
                {
                    field.SetValue(obj,newValue);
                }
                return height;
            }
            else if (field.FieldType.IsArray)
            {
                if(!isDetails)  return height;
                if (value == null)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(GetShowName(field,out _), GUILayout.Width(150));
                    if (GUILayout.Button("New"))
                    {
                        value = Activator.CreateInstance(field.FieldType,0);
                        field.SetValue(obj, value);
                    }
                    EditorGUILayout.EndHorizontal();
                    height += 21;
                    return height;
                }
                else
                {
                    bool foldout = foldoutState.Contains(field);
                    EditorGUILayout.BeginHorizontal();
                    foldout = EditorGUILayout.Foldout(foldout, GetShowName(field,out _));
                    if (GUILayout.Button("置空"))
                    {
                        field.SetValue(obj, default);
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space(1);
                    height += 1;
                    if (foldout)
                    {
                        EditorGUI.indentLevel++;
                        height += DrawFieldArrayInspector(field,obj, value as Array, isDetails);
                        EditorGUI.indentLevel--;
                        foldoutState.Add(field);
                    }
                    else
                    {
                        foldoutState.Remove(field);
                    }
                }
                return height;
            }
            else if (typeof(IList).IsAssignableFrom(field.FieldType) && field.FieldType.IsGenericType)
            {
                if(!isDetails)  return height;
                if (value == null)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(GetShowName(field,out _), GUILayout.Width(150));
                    if (GUILayout.Button("New"))
                    {
                        var newType = listType.MakeGenericType(field.FieldType.GenericTypeArguments);
                        value = Activator.CreateInstance(newType);
                        field.SetValue(obj, value);
                    }
                    EditorGUILayout.EndHorizontal();
                    height += 21;
                    return height;
                }
                else
                {
                    bool foldout = foldoutState.Contains(field);
                    EditorGUILayout.BeginHorizontal();
                    foldout = EditorGUILayout.Foldout(foldout, GetShowName(field,out _));
                    if (GUILayout.Button("置空"))
                    {
                        field.SetValue(obj, default);
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space(1);
                    height += 1;
                    if (foldout)
                    {
                        EditorGUI.indentLevel++;
                        height += DrawFieldListInspector(field, obj, value as IList, isDetails);
                        EditorGUI.indentLevel--;
                        foldoutState.Add(field);
                    }
                    else
                    {
                        foldoutState.Remove(field);
                    }
                }
                return height;
            }
            else if (typeof(IDictionary).IsAssignableFrom(field.FieldType) && field.FieldType.IsGenericType)
            {
                if(!isDetails)  return height;
                if (value == null)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(GetShowName(field,out _), GUILayout.Width(150));
                    if (GUILayout.Button("New"))
                    {
                        var newType = dicType.MakeGenericType(field.FieldType.GenericTypeArguments);
                        value = Activator.CreateInstance(newType);
                        field.SetValue(obj, value);
                    }
                    EditorGUILayout.EndHorizontal();
                    height += 21;
                    return height;
                }
                else
                {
                    bool foldout = foldoutState.Contains(field);
                    EditorGUILayout.BeginHorizontal();
                    foldout = EditorGUILayout.Foldout(foldout, GetShowName(field,out _));
                    if (GUILayout.Button("置空"))
                    {
                        field.SetValue(obj, default);
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space(1);
                    height += 1;
                    if (foldout)
                    {
                        EditorGUI.indentLevel++;
                        height += DrawFieldDictionaryInspector(field, obj, value as IDictionary, isDetails);
                        EditorGUI.indentLevel--;
                        foldoutState.Add(field);
                    }
                    else
                    {
                        foldoutState.Remove(field);
                    }
                }
                return height;
            }
            else if (field.FieldType.IsClass && !field.FieldType.IsGenericType)
            {
                if (value == null)
                {
                    var types = GetSubClassList(field.FieldType,out var names);
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(GetShowName(field,out _), GUILayout.Width(150));
                    var index = EditorGUILayout.Popup(-1, names);
                    EditorGUILayout.EndHorizontal();
                    if (index >= 0)
                    {
                        value = Activator.CreateInstance(types[index]);
                        field.SetValue(obj, value);
                    }
                    height += 21;
                    return height;
                }
                bool foldout = foldoutState.Contains(field);
                EditorGUILayout.BeginHorizontal();
                foldout = EditorGUILayout.Foldout(foldout, GetShowName(field,out _));
                if (GUILayout.Button("置空"))
                {
                    field.SetValue(obj, null);
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(1);
                height += 1;
                if (foldout)
                {
                    EditorGUI.indentLevel++;
                    height += DrawObjectInspector(value, isDetails);
                    EditorGUI.indentLevel--;
                    foldoutState.Add(field);
                }
                else
                {
                    foldoutState.Remove(field);
                }
            }
            else
            {
                return 0;
            }
            height += 20;
            return height + 1;
        }
        protected virtual float ShowNormalField(Type type,string showName, ref object value, bool isDetails = false,FieldInfo field = null)
        {
            float height = 0;
            // 显示字段名称和对应的值
            if (type == typeof(string))
            {
                if (showName == null) value = EditorGUILayout.TextField((string) value);
                else value = EditorGUILayout.TextField(showName, (string) value);
            }
            else if (type == typeof(int))
            {
                int val = 0;
                if (isDetails && field?.GetCustomAttribute(typeof(RangeAttribute)) is RangeAttribute rangeAttr)
                {
                    if(showName==null) val = EditorGUILayout.IntSlider((int) value, Mathf.CeilToInt(rangeAttr.min),
                        Mathf.FloorToInt(rangeAttr.max));
                    else val = EditorGUILayout.IntSlider(showName, (int) value, Mathf.CeilToInt(rangeAttr.min),
                        Mathf.FloorToInt(rangeAttr.max));
                }
                else
                {
                    if(showName==null) val = EditorGUILayout.IntField((int) value);
                    else val = EditorGUILayout.IntField(showName, (int) value);
                }
                if (field?.GetCustomAttribute(typeof(MinAttribute)) is MinAttribute minAttr && val<minAttr.min)
                {
                    val = Mathf.CeilToInt(minAttr.min);
                }
                value = val;
            }
            else if (type == typeof(long))
            {
                long val;
                if(showName==null) val = EditorGUILayout.LongField((long) value);
                else val = EditorGUILayout.LongField(showName, (long) value);
                if (field?.GetCustomAttribute(typeof(RangeAttribute)) is RangeAttribute rangeAttr)
                {
                    if (val < rangeAttr.min) val = (long) Mathf.Ceil(rangeAttr.min);
                    else if (val > rangeAttr.max) val = (long) Mathf.Floor(rangeAttr.max);
                }
                if (field?.GetCustomAttribute(typeof(MinAttribute)) is MinAttribute minAttr && val<minAttr.min)
                {
                    val =  (long) Mathf.Ceil(minAttr.min);
                }
                value = val;
            }
            else if (type == typeof(ulong))
            {
                long val;
                if (showName == null) val = EditorGUILayout.LongField((long) (ulong) value);
                else val = EditorGUILayout.LongField(showName, (long) (ulong) value);
                if (field?.GetCustomAttribute(typeof(RangeAttribute)) is RangeAttribute rangeAttr)
                {
                    if (val < rangeAttr.min) val = (long) Mathf.Ceil(rangeAttr.min);
                    else if (val > rangeAttr.max) val = (long) Mathf.Floor(rangeAttr.max);
                }
                if (field?.GetCustomAttribute(typeof(MinAttribute)) is MinAttribute minAttr && val<minAttr.min)
                {
                    val =  (long) Mathf.Ceil(minAttr.min);
                }
                value = (ulong)val;
            }
            else if (type == typeof(float))
            {
                float val = 0;
                if (isDetails && field?.GetCustomAttribute(typeof(RangeAttribute)) is RangeAttribute rangeAttr)
                {
                    if(showName==null) val = EditorGUILayout.Slider((float) value, rangeAttr.min, rangeAttr.max);
                    else val = EditorGUILayout.Slider(showName, (float) value, rangeAttr.min, rangeAttr.max);
                }
                else
                {
                    if(showName==null) val =  EditorGUILayout.FloatField(showName, (float) value);
                    else val = EditorGUILayout.FloatField(showName, (float) value);
                }
                if (field?.GetCustomAttribute(typeof(MinAttribute)) is MinAttribute minAttr && val<minAttr.min)
                {
                    val = minAttr.min;
                }
                value = val;
            }
            else if (type == typeof(bool))
            {
                if (showName == null) value = EditorGUILayout.Toggle((bool) value);
                else value = EditorGUILayout.Toggle(showName, (bool) value);
            }
            else if (type.IsEnum)
            {
                value = EditorGUILayout.EnumPopup(showName, (Enum) value);
            }
            else if (type == typeof(Vector2))
            {
                value = EditorGUILayout.Vector2Field(showName, (Vector2) value);
                height += 20;
            }
            else if (type == typeof(Vector3))
            {
                value = EditorGUILayout.Vector3Field(showName, (Vector3) value);
                height += 20;
            }
            else if (type == typeof(Vector4))
            {
                value = EditorGUILayout.Vector4Field(showName, (Vector4) value);
                height += 20;
            }
            else if (type == typeof(Rect))
            {
                if (showName == null) value = EditorGUILayout.RectField((Rect) value);
                else value = EditorGUILayout.RectField(showName, (Rect) value);
                height += 40;
            }
            else if (type == typeof(Color))
            {
                if (showName == null) value = EditorGUILayout.ColorField((Color) value);
                else value = EditorGUILayout.ColorField(showName, (Color) value);
            }
            else if (typeof(UnityEngine.Object).IsAssignableFrom(type)
                     && !(field?.GetCustomAttribute(typeof(NotAssetsAttribute)) is NotAssetsAttribute))
            {
                if (typeof(Sprite).IsAssignableFrom(type) ||
                    typeof(Texture).IsAssignableFrom(type))
                    height += 40;
                UnityEngine.Object newObj;
                if (showName == null) newObj = EditorGUILayout.ObjectField((UnityEngine.Object) value, type, false);
                else newObj = EditorGUILayout.ObjectField(showName, (UnityEngine.Object) value, type, false);
                value = newObj;
            }
            else if (type == typeof(AnimationCurve))
            {
                AnimationCurve res;
                if (showName == null) res = EditorGUILayout.CurveField((AnimationCurve) value);
                else res = EditorGUILayout.CurveField(showName, (AnimationCurve) value);
                if (res == null) res = new AnimationCurve();
                value = res;
            }
            else
            {
                return 0;
            }
            height += 20;
            return height + 1;
        }
        
        protected virtual float DrawFieldArrayInspector(FieldInfo field, object obj, Array list, bool isDetails = false)
        {
            float height = 0;
            var len = list.GetLength(0);
            int removeIndex = -1;
            var itemType = TypeHelper.FindType(field.FieldType.FullName?.Replace("[]", ""));
            for (int i = 0; i < len; i++)
            {
                EditorGUILayout.BeginHorizontal();
                var item = list.GetValue(i);
                if (itemType.IsValueType || itemType == stringType)
                {
                    object newValue = item;
                    var itemHeight = ShowNormalField(itemType, i.ToString(), ref newValue, true, field);
                    if (itemHeight <= 0)
                    {
                        EditorGUILayout.LabelField(itemType.FullName);
                    }
                    else if (newValue != item)
                    {
                        list.SetValue(newValue, i);
                    }
                    if (GUILayout.Button("-", GUILayout.Width(40)))
                    {
                        removeIndex = i;
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space(1);
                    height += Mathf.Max(21,itemHeight);
                }
                else
                {
                    if (item == null)
                    {
                        var types = GetSubClassList(itemType, out var names);
                        EditorGUILayout.LabelField(i.ToString(), GUILayout.Width(150));
                        var index = EditorGUILayout.Popup(-1, names);
                        if (GUILayout.Button("-",GUILayout.Width(40)))
                        {
                            removeIndex = i;
                        }
                        EditorGUILayout.EndHorizontal();
                        if (index >= 0)
                        {
                            item = Activator.CreateInstance(types[index]);
                            list.SetValue(item, i);
                        }
                        else
                        {
                            height += 20;
                        }
                        continue;
                    }

                    if (!listFoldoutState.TryGetValue(field, out var listFoldout))
                    {
                        listFoldout = new();
                        listFoldoutState.Add(field,listFoldout);
                    }

                    bool foldout = listFoldout.Contains(i);
                
                    foldout = EditorGUILayout.Foldout(foldout,GetShowName(item.GetType(), out _));
                    if (GUILayout.Button("置空",GUILayout.Width(50)))
                    {
                        list.SetValue(null, i);
                    }

                    if (GUILayout.Button("-",GUILayout.Width(40)))
                    {
                        removeIndex = i;
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space(1);
                    height += 1;
                    if (foldout)
                    {
                        height += DrawObjectInspector(item, isDetails);
                        listFoldout.Add(i);
                    }
                    else
                    {
                        listFoldout.Remove(i);
                    }
                }
            }
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("");
            if (GUILayout.Button("+", GUILayout.Width(40)))
            {
                var value = Activator.CreateInstance(field.FieldType, len + 1) as Array;
                Array.Copy(list, value, len);
                field.SetValue(obj, value);
            }

            if (removeIndex >= 0)
            {
                var value = Activator.CreateInstance(field.FieldType, len - 1) as Array;
                if (removeIndex > 0)
                {
                    Array.Copy(list, value, removeIndex);
                }

                if (removeIndex < len)
                {
                    Array.Copy(list, removeIndex + 1, value, removeIndex, len - 1 - removeIndex);
                }

                field.SetValue(obj, value);
            }
            EditorGUILayout.EndHorizontal();
            height += 20;
            return height;
        }
        
        protected virtual float DrawFieldListInspector(FieldInfo field, object obj, IList list, bool isDetails = false)
        {
            float height = 0;
            var len = list.Count;
            int removeIndex = -1;
            var itemType = field.FieldType.GenericTypeArguments[0];
            for (int i = 0; i < len; i++)
            {
                EditorGUILayout.BeginHorizontal();
                var item = list[i];
                if (itemType.IsValueType || itemType == stringType)
                {
                    object newValue = item;
                    var itemHeight = ShowNormalField(itemType, i.ToString(), ref newValue, true, field);
                    if (itemHeight <= 0)
                    {
                        EditorGUILayout.LabelField(itemType.FullName);
                    }
                    else if (newValue != item)
                    {
                        list[i] = newValue;
                    }
                    if (GUILayout.Button("-", GUILayout.Width(40)))
                    {
                        removeIndex = i;
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space(1);
                    height += Mathf.Max(21,itemHeight);
                }
                else
                {
                    if (item == null)
                    {
                        var types = GetSubClassList(itemType, out var names);
                        EditorGUILayout.LabelField(i.ToString(), GUILayout.Width(150));
                        var index = EditorGUILayout.Popup(-1, names);
                        if (GUILayout.Button("-",GUILayout.Width(40)))
                        {
                            removeIndex = i;
                        }
                        EditorGUILayout.EndHorizontal();
                        if (index >= 0)
                        {
                            item = Activator.CreateInstance(types[index]);
                            list[i] = item;
                        }
                        else
                        {
                            height += 20;
                        }
                        continue;
                    }

                    if (!listFoldoutState.TryGetValue(field, out var listFoldout))
                    {
                        listFoldout = new();
                        listFoldoutState.Add(field,listFoldout);
                    }

                    bool foldout = listFoldout.Contains(i);
                
                    foldout = EditorGUILayout.Foldout(foldout,GetShowName(item.GetType(), out _));
                    if (GUILayout.Button("置空",GUILayout.Width(50)))
                    {
                        list[i] = null;
                    }

                    if (GUILayout.Button("-",GUILayout.Width(40)))
                    {
                        removeIndex = i;
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space(1);
                    height += 1;
                    if (foldout)
                    {
                        height += DrawObjectInspector(item, isDetails);
                        listFoldout.Add(i);
                    }
                    else
                    {
                        listFoldout.Remove(i);
                    }
                }
            }
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("");
            if (GUILayout.Button("+", GUILayout.Width(40)))
            {
                list.Add(itemType.IsValueType ? Activator.CreateInstance(itemType) : null);
            }
            if (removeIndex >= 0)
            {
                list.RemoveAt(removeIndex);
            }
            EditorGUILayout.EndHorizontal();
            height += 20;
            return height;
        }

        protected virtual float DrawFieldDictionaryInspector(FieldInfo field, object obj, IDictionary dictionary, bool isDetails = false)
        {
            float height = 0;
            int i = 0;
            object removeKey = null;
            object editorKey = null;
            object changeValue = null;
            var keyType = field.FieldType.GenericTypeArguments[0];
            var itemType = field.FieldType.GenericTypeArguments[1];
            foreach (DictionaryEntry kv in dictionary)
            {
                i++;
                EditorGUILayout.BeginHorizontal();
                var item = kv.Value;
                var key = kv.Key;
                float keyHeight = 21;
                float valueHeight = 0;
                bool foldout = false;
                if (!(itemType.IsValueType || keyType == stringType) && item != null)
                {
                    if (!listFoldoutState.TryGetValue(field, out var listFoldout))
                    {
                        listFoldout = new();
                        listFoldoutState.Add(field,listFoldout);
                    }
                    foldout = listFoldout.Contains(i);
                    foldout = EditorGUILayout.Foldout(foldout,"");
                    if (keyType.IsValueType || keyType == stringType)
                    {
                        GUILayout.TextField(key.ToString());
                    }
                    else
                    {
                        GUILayout.TextField(keyType.FullName);
                    }
                }
                else
                {
                    if (keyType.IsValueType || keyType == stringType)
                    {
                        EditorGUILayout.LabelField(key.ToString());
                    }
                    else
                    {
                        EditorGUILayout.LabelField(keyType.FullName);
                    }
                }
                if (itemType.IsValueType || itemType == stringType)
                {
                    var newItem = item;
                    var itemHeight = ShowNormalField(itemType, null, ref newItem, true);
                    if (itemHeight <= 0)
                    {
                        EditorGUILayout.LabelField(item.ToString());
                    }
                    else if (newItem != item)
                    {
                        editorKey = key;
                        changeValue = newItem;
                    }
                    if (GUILayout.Button("-", GUILayout.Width(40)))
                    {
                        removeKey = kv.Key;
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space(1);
                    valueHeight += 1;
                }
                else
                {
                    if (item == null)
                    {
                        var types = GetSubClassList(itemType, out var names);
                        var index = EditorGUILayout.Popup(-1, names);
                        if (GUILayout.Button("-", GUILayout.Width(40)))
                        {
                            removeKey = kv.Key;
                        }

                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.Space(1);
                        valueHeight += 1;
                        if (index >= 0)
                        {
                            editorKey = key;
                            changeValue = Activator.CreateInstance(types[index]);
                        }
                        else
                        {
                            valueHeight += 21;
                        }
                    }
                    else
                    {
                        if (!listFoldoutState.TryGetValue(field, out var listFoldout))
                        {
                            listFoldout = new();
                            listFoldoutState.Add(field,listFoldout);
                        }
                        if (GUILayout.Button("置空",GUILayout.Width(50)))
                        {
                            changeValue = null;
                            editorKey = key;
                        }

                        if (GUILayout.Button("-",GUILayout.Width(40)))
                        {
                            removeKey = key;
                        }
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.Space(1);
                        valueHeight += 1;
                        if (foldout)
                        {
                            valueHeight += DrawObjectInspector(item, isDetails);
                            listFoldout.Add(i);
                        }
                        else
                        {
                            listFoldout.Remove(i);
                        }
                    }
                   
                }
                height += Mathf.Max(keyHeight, valueHeight);
            }
            EditorGUILayout.BeginHorizontal();
            object inputKey;
            if (keyType.IsValueType || keyType == stringType)
            {
                if (!dicInputKey.TryGetValue(field, out inputKey))
                {
                    inputKey = keyType == stringType?"":Activator.CreateInstance(keyType);
                    dicInputKey.Add(field,inputKey);
                }
                var itemHeight = ShowNormalField(keyType, "AddKey", ref inputKey, true, field);
                if (itemHeight <= 0)
                {
                    EditorGUILayout.LabelField(keyType.FullName);
                }
            }
            else
            {
                inputKey = keyType == stringType?"":Activator.CreateInstance(keyType);
            }
            dicInputKey[field] = inputKey;
            if (GUILayout.Button("+", GUILayout.Width(40)))
            {
                if (!dictionary.Contains(inputKey))
                {
                    dictionary.Add(inputKey, itemType.IsValueType ? Activator.CreateInstance(itemType) : null);
                    dicInputKey.Remove(field);
                }
                else
                {
                    Debug.LogError("Key已存在");
                }
            }
            if (removeKey != null)
            {
                dictionary.Remove(removeKey);
            }
            if (editorKey != null)
            {
                dictionary[editorKey] = changeValue;
            }
            EditorGUILayout.EndHorizontal();
            return height;
        }
        protected virtual string GetShowName(FieldInfo field,out bool rename)
        {
            rename = false;
            return ObjectNames.NicifyVariableName(field.Name);
        }
        
        protected virtual string GetShowName(Type type, out bool rename)
        {
            rename = false;
            return ObjectNames.NicifyVariableName(type.Name);
        }

        protected virtual List<Type> GetSubClassList(Type type, out string[] names)
        {
            return TypeHelper.GetSubClassList(type, out names);
        }
    }
}