using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace DaGenGraph.Editor
{
    public abstract class DrawBase: EditorWindow
    {
        private HashSet<FieldInfo> foldoutState = new HashSet<FieldInfo>();
        private Dictionary<FieldInfo,HashSet<int>> listFoldoutState = new Dictionary<FieldInfo,HashSet<int>>();
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
            float height = 0;
            object value = field.GetValue(obj);
            // 显示字段名称和对应的值
            if (field.FieldType == typeof(string))
            {
                field.SetValue(obj, EditorGUILayout.TextField(GetShowName(field,out _), (string) value));
            }
            else if (field.FieldType == typeof(int))
            {
                int val = 0;
                if (isDetails && field.GetCustomAttribute(typeof(RangeAttribute)) is RangeAttribute rangeAttr)
                {
                    val = EditorGUILayout.IntSlider(GetShowName(field,out _), (int) value, Mathf.CeilToInt(rangeAttr.min),
                        Mathf.FloorToInt(rangeAttr.max));
                }
                else
                {
                    val = EditorGUILayout.IntField(GetShowName(field,out _), (int) value);
                }
                var min = field.GetCustomAttribute(typeof(MinAttribute));
                if (min is MinAttribute minAttr && val<minAttr.min)
                {
                    val = Mathf.CeilToInt(minAttr.min);
                }
                field.SetValue(obj, val);
            }
            else if (field.FieldType == typeof(long))
            {
                long val = EditorGUILayout.LongField(GetShowName(field,out _), (long) value);
                if (field.GetCustomAttribute(typeof(RangeAttribute)) is RangeAttribute rangeAttr)
                {
                    if (val < rangeAttr.min) val = (long) Mathf.Ceil(rangeAttr.min);
                    else if (val > rangeAttr.max) val = (long) Mathf.Floor(rangeAttr.max);
                }
                var min = field.GetCustomAttribute(typeof(MinAttribute));
                if (min is MinAttribute minAttr && val<minAttr.min)
                {
                    val =  (long) Mathf.Ceil(minAttr.min);
                }
                field.SetValue(obj, val);
            }
            else if (field.FieldType == typeof(ulong))
            {
                long val = EditorGUILayout.LongField(GetShowName(field,out _), (long) (ulong) value);
                if (field.GetCustomAttribute(typeof(RangeAttribute)) is RangeAttribute rangeAttr)
                {
                    if (val < rangeAttr.min) val = (long) Mathf.Ceil(rangeAttr.min);
                    else if (val > rangeAttr.max) val = (long) Mathf.Floor(rangeAttr.max);
                }
                var min = field.GetCustomAttribute(typeof(MinAttribute));
                if (min is MinAttribute minAttr && val<minAttr.min)
                {
                    val =  (long) Mathf.Ceil(minAttr.min);
                }
                field.SetValue(obj, (ulong)val);
            }
            else if (field.FieldType == typeof(float))
            {
                float val = 0;
                if (isDetails && field.GetCustomAttribute(typeof(RangeAttribute)) is RangeAttribute rangeAttr)
                {
                    val = EditorGUILayout.Slider(GetShowName(field,out _), (float) value, rangeAttr.min, rangeAttr.max);
                }
                else
                {
                    val = EditorGUILayout.FloatField(GetShowName(field,out _), (float) value);
                }
                var min = field.GetCustomAttribute(typeof(MinAttribute));
                if (min is MinAttribute minAttr && val<minAttr.min)
                {
                    val = minAttr.min;
                }
                field.SetValue(obj, val);
            }
            else if (field.FieldType == typeof(bool))
            {
                field.SetValue(obj, EditorGUILayout.Toggle(GetShowName(field,out _), (bool) value));
            }
            else if (field.FieldType.IsEnum)
            {
                field.SetValue(obj, EditorGUILayout.EnumPopup(GetShowName(field,out _), (Enum) value));
            }
            else if (field.FieldType == typeof(Vector2))
            {
                field.SetValue(obj, EditorGUILayout.Vector2Field(GetShowName(field,out _), (Vector2) value));
                height += 20;
            }
            else if (field.FieldType == typeof(Vector3))
            {
                field.SetValue(obj, EditorGUILayout.Vector3Field(GetShowName(field,out _), (Vector3) value));
                height += 20;
            }
            else if (field.FieldType == typeof(Vector4))
            {
                field.SetValue(obj, EditorGUILayout.Vector4Field(GetShowName(field,out _), (Vector4) value));
                height += 20;
            }
            else if (field.FieldType == typeof(Rect))
            {
                field.SetValue(obj, EditorGUILayout.RectField(GetShowName(field,out _), (Rect) value));
                height += 40;
            }
            else if (field.FieldType == typeof(Color))
            {
                field.SetValue(obj, EditorGUILayout.ColorField(GetShowName(field,out _), (Color) value));
            }
            else if (typeof(UnityEngine.Object).IsAssignableFrom(field.FieldType)
                     && !(field.GetCustomAttribute(typeof(NotAssetsAttribute)) is NotAssetsAttribute))
            {
                if (typeof(Sprite).IsAssignableFrom(field.FieldType) ||
                    typeof(Texture).IsAssignableFrom(field.FieldType))
                    height += 40;
                var newObj = EditorGUILayout.ObjectField(GetShowName(field,out _), (UnityEngine.Object) value, field.FieldType, false);
                field.SetValue(obj, newObj);
            }
            else if (field.FieldType == typeof(AnimationCurve))
            {
                var res = EditorGUILayout.CurveField(GetShowName(field,out _), (AnimationCurve) value);
                if (res == null) res = new AnimationCurve();
                field.SetValue(obj, res);
            }
            else if (field.FieldType.IsArray)
            {
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
                        field.SetValue(obj, null);
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
            else if (field.FieldType.IsClass && !field.FieldType.IsGenericType)
            {
                if (value == null)
                {
                    var types = TypeHelper.GetSubClassList(field.FieldType,out var names);
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(GetShowName(field,out _), GUILayout.Width(150));
                    var index = EditorGUILayout.Popup(-1, names);
                    EditorGUILayout.EndHorizontal();
                    if (index >= 0)
                    {
                        value = Activator.CreateInstance(types[index]);
                        field.SetValue(obj, value);
                    }
                    else
                    {
                        return 20;
                    }
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
        
        protected virtual float DrawFieldArrayInspector(FieldInfo field, object obj, Array list, bool isDetails = false)
        {
            float height = 0;
            var len = list.GetLength(0);
            int removeIndex = -1;
            for (int i = 0; i < len; i++)
            {
                EditorGUILayout.BeginHorizontal();
                var item = list.GetValue(i);
                if (item == null)
                {
                    var itemType = TypeHelper.FindType(field.FieldType.FullName.Replace("[]", ""));
                    var types = TypeHelper.GetSubClassList(itemType, out var names);
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
    }
}