using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace DaGenGraph.Editor
{
    public abstract class DrawBase : EditorWindow
    {
        private static List<ISort> sortTemp = new();
        private static Dictionary<string, GroupItem> groupsTemp = new();

        private static Type stringType = typeof(string);
        private static Type listType = typeof(List<>);
        private static Type dicType = typeof(Dictionary<,>);
        private static Type objectType = typeof(UnityEngine.Object);

        private static Dictionary<Type, ISort[]> sortsMap = new();


        private HashSet<FieldInfo> foldoutState = new();
        private HashSet<GroupItem> foldoutState2 = new();

        private static Dictionary<FieldInfo, HashSet<int>> listFoldoutState = new();
        private static Dictionary<FieldInfo, object> dicInputKey = new();
        private static Dictionary<Type, string[]> enumDropDown = new();

        private static Dictionary<FieldInfo, IValueDropdownItem[]> valueDropdown = new();

        private static List<IValueDropdownItem> temp = new();
        private static List<Type> temp2 = new();
        private static List<string> temp3 = new();

        protected virtual float DrawObjectInspector(object obj, bool isDetails = false)
        {
            float height = 0;
            if (obj == null) return height;
            var members = GetSortMember(obj);

            foreach (var member in members)
            {
                if (member is MemberItem memberItem)
                {
                    height += DrawMemberInspector(memberItem.Member, obj, isDetails);
                }
                else if (member is GroupItem groupItem)
                {
                    height += 31;
                    EditorGUILayout.Space(5);
                    bool fold = foldoutState2.Contains(groupItem);
                    if (!isDetails)
                        fold = EditorGUILayout.BeginFoldoutHeaderGroup(fold, groupItem.GroupId);
                    else
                        fold = EditorGUILayout.Foldout(fold, groupItem.GroupId);
                    if (fold)
                    {
                        for (int i = 0; i < groupItem.Members.Count; i++)
                        {
                            height += DrawMemberInspector(groupItem.Members[i].Member, obj, isDetails);
                        }
                    }

                    if (!isDetails)
                        EditorGUILayout.EndFoldoutHeaderGroup();
                    if (!fold)
                    {
                        foldoutState2.Remove(groupItem);
                    }
                    else
                    {
                        foldoutState2.Add(groupItem);
                    }

                    EditorGUILayout.Space(5);
                }
            }

            return height;
        }

        protected virtual float DrawMemberInspector(MemberInfo member, object obj, bool isDetails = false)
        {
            float height = 0;
            if (!NeedShowInspector(member, obj, isDetails)) return 0;

            if (isDetails && member.GetCustomAttribute(typeof(InfoBoxAttribute)) is InfoBoxAttribute infoBox)
            {
                EditorGUILayout.HelpBox(infoBox.Message, (MessageType) (int) infoBox.InfoMessageType);
                height += 40;
            }

            if (member.GetCustomAttribute(typeof(HeaderAttribute)) is HeaderAttribute header)
            {
                EditorGUILayout.LabelField(header.header);
                height += 20;
            }

            if (member.GetCustomAttribute(typeof(SpaceAttribute)) is SpaceAttribute space)
            {
                EditorGUILayout.Space(space.height);
                height += space.height;
            }

            bool disable = false;
            if (member.GetCustomAttribute(typeof(ReadOnlyAttribute)) is ReadOnlyAttribute ||
                member.GetCustomAttribute(typeof(DisableInEditorModeAttribute)) is DisableInEditorModeAttribute)
            {
                disable = true;
                EditorGUI.BeginDisabledGroup(true);
            }

            if (member is FieldInfo field)
            {
                OnValueChangedAttribute attribute = null;
                object value = null;
                if (field.GetCustomAttribute(typeof(OnValueChangedAttribute)) is OnValueChangedAttribute
                    valueChangedAttribute)
                {
                    value = field.GetValue(obj);
                    attribute = valueChangedAttribute;
                }

                height += DrawFieldInspector(field, obj, isDetails);
                if (attribute != null)
                {
                    var newValue = field.GetValue(obj);
                    if (!IsEqual(newValue, value))
                    {
                        var method = field.DeclaringType.GetMethod(attribute.Action,
                            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                        //仅支持无参方法
                        if (method != null && method.GetParameters().Length <= 0)
                        {
                            method.Invoke(obj, null);
                        }
                    }
                }

            }
            else if (member is MethodInfo method)
            {
                height += DrawMethodInspector(method, obj, isDetails);
            }

            if (disable)
            {
                EditorGUI.EndDisabledGroup();
            }
            else
            {
                if (member.GetCustomAttribute(typeof(OnStateUpdateAttribute)) is OnStateUpdateAttribute
                    stateUpdateAttribute)
                {
                    var method = member.DeclaringType.GetMethod(stateUpdateAttribute.Action,
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    //仅支持无参方法
                    if (method != null && method.GetParameters().Length <= 0)
                    {
                        method.Invoke(obj, null);
                    }
                }
            }

            return height;
        }

        protected virtual bool NeedShowInspector(MemberInfo member, object obj, bool isDetails)
        {
            if (!SelectMemberInfo(member, obj, isDetails)) return false;
            if (member.GetCustomAttribute(typeof(DrawIgnoreAttribute)) is DrawIgnoreAttribute ignoreAttribute)
            {
                if (ignoreAttribute.Ignore == Ignore.All) return false;
                if (ignoreAttribute.Ignore == Ignore.Details == isDetails) return false;
            }

            if (member.GetCustomAttribute(typeof(ShowIfAttribute)) is ShowIfAttribute showIfAttribute)
            {
                if (!CheckShowIf(member, obj, showIfAttribute)) return false;
            }
            return true;
        }
        #region DrawField

        protected virtual float DrawFieldInspector(FieldInfo field, object obj, bool isDetails = false)
        {
            object value = field.GetValue(obj);
            object newValue = value;
            float height = 0;
            if (field.GetCustomAttribute(typeof(ValueDropdownAttribute)) is ValueDropdownAttribute
                valueDropdownAttribute)
            {
                return DrawValueDropdownFieldInspector(field, obj, valueDropdownAttribute);
            }

            if (field.FieldType.IsEnum)
            {
                return DrawEnumFieldInspector(field, obj);
            }

            if (field.FieldType.IsArray)
            {
                if (!isDetails) return height;
                if (value == null)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(GetShowName(field), GUILayout.Width(150));
                    if (GUILayout.Button("New"))
                    {
                        value = Activator.CreateInstance(field.FieldType, 0);
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
                    foldout = EditorGUILayout.Foldout(foldout, GetShowName(field, value));
                    if (field.GetCustomAttribute(typeof(HideReferenceObjectPickerAttribute)) == null)
                    {
                        if (GUILayout.Button("置空"))
                        {
                            field.SetValue(obj, default);
                        }
                    }

                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space(1);
                    height += 1;
                    if (foldout)
                    {
                        EditorGUI.indentLevel++;
                        height += DrawFieldArrayInspector(field, obj, value as Array, isDetails);
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

            if (typeof(IList).IsAssignableFrom(field.FieldType) && field.FieldType.IsGenericType)
            {
                if (!isDetails) return height;
                if (value == null)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(GetShowName(field), GUILayout.Width(150));
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
                    foldout = EditorGUILayout.Foldout(foldout, GetShowName(field, value));
                    if (field.GetCustomAttribute(typeof(HideReferenceObjectPickerAttribute)) == null)
                    {
                        if (GUILayout.Button("置空"))
                        {
                            field.SetValue(obj, default);
                        }
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

            if (typeof(IDictionary).IsAssignableFrom(field.FieldType) && field.FieldType.IsGenericType)
            {
                if (!isDetails) return height;
                if (value == null)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(GetShowName(field), GUILayout.Width(150));
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
                    foldout = EditorGUILayout.Foldout(foldout, GetShowName(field, value));
                    if (field.GetCustomAttribute(typeof(HideReferenceObjectPickerAttribute)) == null)
                    {
                        if (GUILayout.Button("置空"))
                        {
                            field.SetValue(obj, default);
                        }
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

            // 显示字段名称和对应的值
            height += DrawNormalField(field.FieldType, GetShowName(field, value), ref newValue, isDetails, field);
            if (height > 0)
            {
                if (!IsEqual(value, newValue))
                {
                    field.SetValue(obj, newValue);
                }

                return height;
            }

            if (field.FieldType.IsClass
                && field.FieldType != stringType
                && !field.FieldType.IsGenericType)
            {
                if (value == null)
                {
                    var types = GetSubClassList(field, obj, field.FieldType, out var names);
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(GetShowName(field), GUILayout.Width(150));
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
                foldout = EditorGUILayout.Foldout(foldout, GetShowName(field, value));
                if (field.GetCustomAttribute(typeof(HideReferenceObjectPickerAttribute)) == null)
                {
                    if (GUILayout.Button("置空"))
                    {
                        field.SetValue(obj, null);
                    }
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

                height += 21;
                return height;
            }

            return height;
        }

        protected virtual float DrawNormalField(Type type, GUIContent showName, ref object value,
            bool isDetails = false, FieldInfo field = null)
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
                    if (showName == null)
                        val = EditorGUILayout.IntSlider((int) value, Mathf.CeilToInt(rangeAttr.min),
                            Mathf.FloorToInt(rangeAttr.max));
                    else
                        val = EditorGUILayout.IntSlider(showName, (int) value, Mathf.CeilToInt(rangeAttr.min),
                            Mathf.FloorToInt(rangeAttr.max));
                }
                else
                {
                    if (showName == null) val = EditorGUILayout.IntField((int) value);
                    else val = EditorGUILayout.IntField(showName, (int) value);
                }

                if (field?.GetCustomAttribute(typeof(MinAttribute)) is MinAttribute minAttr && val < minAttr.min)
                {
                    val = Mathf.CeilToInt(minAttr.min);
                }

                if (field?.GetCustomAttribute(typeof(MinValueAttribute)) is MinValueAttribute minVAttr &&
                    val < minVAttr.MinValue)
                {
                    if (minVAttr.MinValue < int.MinValue)
                        val = int.MinValue;
                    else
                        val = (int) Math.Ceiling(minVAttr.MinValue);
                }

                if (field?.GetCustomAttribute(typeof(MaxValueAttribute)) is MaxValueAttribute maxVAttr &&
                    val > maxVAttr.MaxValue)
                {
                    if (maxVAttr.MaxValue > int.MaxValue)
                        val = int.MaxValue;
                    else
                        val = (int) Math.Floor(maxVAttr.MaxValue);
                }

                value = val;
            }
            else if (type == typeof(long))
            {
                long val;
                if (showName == null) val = EditorGUILayout.LongField((long) value);
                else val = EditorGUILayout.LongField(showName, (long) value);
                if (field?.GetCustomAttribute(typeof(RangeAttribute)) is RangeAttribute rangeAttr)
                {
                    if (val < rangeAttr.min) val = (long) Mathf.Ceil(rangeAttr.min);
                    else if (val > rangeAttr.max) val = (long) Mathf.Floor(rangeAttr.max);
                }

                if (field?.GetCustomAttribute(typeof(MinAttribute)) is MinAttribute minAttr && val < minAttr.min)
                {
                    val = (long) Mathf.Ceil(minAttr.min);
                }

                if (field?.GetCustomAttribute(typeof(MinValueAttribute)) is MinValueAttribute minVAttr &&
                    val < minVAttr.MinValue)
                {
                    if (minVAttr.MinValue < long.MinValue)
                        val = long.MinValue;
                    else
                        val = (long) Math.Ceiling(minVAttr.MinValue);
                }

                if (field?.GetCustomAttribute(typeof(MaxValueAttribute)) is MaxValueAttribute maxVAttr &&
                    val > maxVAttr.MaxValue)
                {
                    if (maxVAttr.MaxValue > long.MaxValue)
                        val = long.MaxValue;
                    else
                        val = (long) Math.Floor(maxVAttr.MaxValue);
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

                if (field?.GetCustomAttribute(typeof(MinAttribute)) is MinAttribute minAttr && val < minAttr.min)
                {
                    val = (long) Mathf.Ceil(minAttr.min);
                }

                if (field?.GetCustomAttribute(typeof(MinValueAttribute)) is MinValueAttribute minVAttr &&
                    val < minVAttr.MinValue)
                {
                    if (minVAttr.MinValue < long.MinValue)
                        val = long.MinValue;
                    else
                        val = (long) Math.Ceiling(minVAttr.MinValue);
                }

                if (field?.GetCustomAttribute(typeof(MaxValueAttribute)) is MaxValueAttribute maxVAttr &&
                    val > maxVAttr.MaxValue)
                {
                    if (maxVAttr.MaxValue > long.MaxValue)
                        val = long.MaxValue;
                    else
                        val = (long) Math.Floor(maxVAttr.MaxValue);
                }

                value = (ulong) val;
            }
            else if (type == typeof(float))
            {
                float val = 0;
                if (isDetails && field?.GetCustomAttribute(typeof(RangeAttribute)) is RangeAttribute rangeAttr)
                {
                    if (showName == null) val = EditorGUILayout.Slider((float) value, rangeAttr.min, rangeAttr.max);
                    else val = EditorGUILayout.Slider(showName, (float) value, rangeAttr.min, rangeAttr.max);
                }
                else
                {
                    if (showName == null) val = EditorGUILayout.FloatField(showName, (float) value);
                    else val = EditorGUILayout.FloatField(showName, (float) value);
                }

                if (field?.GetCustomAttribute(typeof(MinAttribute)) is MinAttribute minAttr && val < minAttr.min)
                {
                    val = minAttr.min;
                }

                if (field?.GetCustomAttribute(typeof(MinValueAttribute)) is MinValueAttribute minVAttr &&
                    val < minVAttr.MinValue)
                {
                    if (minVAttr.MinValue < float.MinValue)
                        val = float.MinValue;
                    else
                        val = (float) Math.Ceiling(minVAttr.MinValue);
                }

                if (field?.GetCustomAttribute(typeof(MaxValueAttribute)) is MaxValueAttribute maxVAttr &&
                    val > maxVAttr.MaxValue)
                {
                    if (maxVAttr.MaxValue > float.MaxValue)
                        val = float.MaxValue;
                    else
                        val = (float) Math.Floor(maxVAttr.MaxValue);
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
            else if (objectType.IsAssignableFrom(type)
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
            bool change = false;
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
                    var itemHeight = DrawNormalField(itemType, new GUIContent(i.ToString()), ref newValue, true, field);
                    if (itemHeight <= 0)
                    {
                        EditorGUILayout.LabelField(itemType.FullName);
                    }
                    else if (!IsEqual(newValue, item))
                    {
                        list.SetValue(newValue, i);
                    }

                    if (GUILayout.Button("-", GUILayout.Width(40)))
                    {
                        removeIndex = i;
                    }

                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space(1);
                    height += Mathf.Max(21, itemHeight);
                }
                else
                {
                    if (item == null)
                    {
                        var types = GetSubClassList(field, obj, itemType, out var names);
                        EditorGUILayout.LabelField(i.ToString(), GUILayout.Width(150));
                        var index = EditorGUILayout.Popup(-1, names);
                        if (GUILayout.Button("-", GUILayout.Width(40)))
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
                        listFoldoutState.Add(field, listFoldout);
                    }

                    bool foldout = listFoldout.Contains(i);

                    foldout = EditorGUILayout.Foldout(foldout, GetShowName(item.GetType()));
                    if (GUILayout.Button("置空", GUILayout.Width(50)))
                    {
                        list.SetValue(null, i);
                    }

                    if (GUILayout.Button("-", GUILayout.Width(40)))
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
                change = true;
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
                change = true;
            }

            EditorGUILayout.EndHorizontal();
            height += 20;
            if (change &&
                field.GetCustomAttribute(typeof(OnCollectionChangedAttribute)) is OnCollectionChangedAttribute
                    collectionChangedAttribute)
            {
                var method = field.DeclaringType.GetMethod(collectionChangedAttribute.After,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                //仅支持无参方法
                if (method != null && method.GetParameters().Length <= 0)
                {
                    method.Invoke(obj, null);
                }
            }

            return height;
        }

        protected virtual float DrawFieldListInspector(FieldInfo field, object obj, IList list, bool isDetails = false)
        {
            bool change = false;
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
                    var itemHeight = DrawNormalField(itemType, new GUIContent(i.ToString()), ref newValue, true, field);
                    if (itemHeight <= 0)
                    {
                        EditorGUILayout.LabelField(itemType.FullName);
                    }
                    else if (!IsEqual(newValue, item))
                    {
                        list[i] = newValue;
                    }

                    if (GUILayout.Button("-", GUILayout.Width(40)))
                    {
                        removeIndex = i;
                    }

                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space(1);
                    height += Mathf.Max(21, itemHeight);
                }
                else
                {
                    if (item == null)
                    {
                        var types = GetSubClassList(field, obj, itemType, out var names);
                        EditorGUILayout.LabelField(i.ToString(), GUILayout.Width(150));
                        var index = EditorGUILayout.Popup(-1, names);
                        if (GUILayout.Button("-", GUILayout.Width(40)))
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
                        listFoldoutState.Add(field, listFoldout);
                    }

                    bool foldout = listFoldout.Contains(i);

                    foldout = EditorGUILayout.Foldout(foldout, GetShowName(item.GetType()));
                    if (GUILayout.Button("置空", GUILayout.Width(50)))
                    {
                        list[i] = null;
                    }

                    if (GUILayout.Button("-", GUILayout.Width(40)))
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
                change = true;
            }

            if (removeIndex >= 0)
            {
                list.RemoveAt(removeIndex);
                change = true;
            }

            EditorGUILayout.EndHorizontal();
            height += 20;
            if (change &&
                field.GetCustomAttribute(typeof(OnCollectionChangedAttribute)) is OnCollectionChangedAttribute
                    collectionChangedAttribute)
            {
                var method = field.DeclaringType.GetMethod(collectionChangedAttribute.After,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                //仅支持无参方法
                if (method != null && method.GetParameters().Length <= 0)
                {
                    method.Invoke(obj, null);
                }
            }

            return height;
        }

        protected virtual float DrawFieldDictionaryInspector(FieldInfo field, object obj, IDictionary dictionary,
            bool isDetails = false)
        {
            float height = 0;
            int i = 0;
            object removeKey = null;
            object editorKey = null;
            object changeValue = null;
            var keyType = field.FieldType.GenericTypeArguments[0];
            var itemType = field.FieldType.GenericTypeArguments[1];
            bool change = false;
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
                        listFoldoutState.Add(field, listFoldout);
                    }

                    foldout = listFoldout.Contains(i);
                    foldout = EditorGUILayout.Foldout(foldout, "");
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
                    var itemHeight = DrawNormalField(itemType, null, ref newItem, true);
                    if (itemHeight <= 0)
                    {
                        EditorGUILayout.LabelField(item.ToString());
                    }
                    else if (!IsEqual(newItem, item))
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
                        var types = GetSubClassList(field, obj, itemType, out var names);
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
                            listFoldoutState.Add(field, listFoldout);
                        }

                        if (GUILayout.Button("置空", GUILayout.Width(50)))
                        {
                            changeValue = null;
                            editorKey = key;
                        }

                        if (GUILayout.Button("-", GUILayout.Width(40)))
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
                    inputKey = keyType == stringType ? "" : Activator.CreateInstance(keyType);
                    dicInputKey.Add(field, inputKey);
                }

                var itemHeight = DrawNormalField(keyType, new GUIContent("AddKey"), ref inputKey, true, field);
                if (itemHeight <= 0)
                {
                    EditorGUILayout.LabelField(keyType.FullName);
                }
            }
            else
            {
                inputKey = keyType == stringType ? "" : Activator.CreateInstance(keyType);
            }

            dicInputKey[field] = inputKey;
            if (GUILayout.Button("+", GUILayout.Width(40)))
            {
                if (!dictionary.Contains(inputKey))
                {
                    dictionary.Add(inputKey, itemType.IsValueType ? Activator.CreateInstance(itemType) : null);
                    dicInputKey.Remove(field);
                    change = true;
                }
                else
                {
                    Debug.LogError("Key已存在");
                }
            }

            if (removeKey != null)
            {
                dictionary.Remove(removeKey);
                change = true;
            }

            if (editorKey != null)
            {
                dictionary[editorKey] = changeValue;
            }

            EditorGUILayout.EndHorizontal();
            if (change &&
                field.GetCustomAttribute(typeof(OnCollectionChangedAttribute)) is OnCollectionChangedAttribute
                    collectionChangedAttribute)
            {
                var method = field.DeclaringType.GetMethod(collectionChangedAttribute.After,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                //仅支持无参方法
                if (method != null && method.GetParameters().Length <= 0)
                {
                    method.Invoke(obj, null);
                }
            }

            return height;
        }

        protected virtual float DrawEnumFieldInspector(FieldInfo field, object obj)
        {
            if (!enumDropDown.TryGetValue(field.FieldType, out var names))
            {
                names = Enum.GetNames(field.FieldType);
                bool has = false;
                for (int i = 0; i < names.Length; i++)
                {
                    var enumField = field.FieldType.GetField(names[i]);
                    names[i] = GetShowNameString(enumField, out bool rename);
                    has |= rename;
                }

                if (!has)
                {
                    names = null;
                }

                enumDropDown.Add(field.FieldType, names);
            }

            if (names == null)
            {
                var value = field.GetValue(obj);
                field.SetValue(obj, EditorGUILayout.EnumPopup(GetShowName(field, value), (Enum) value));
            }
            else
            {
                var value = field.GetValue(obj);
                int index = -1;
                var list = Enum.GetValues(field.FieldType);
                for (int i = 0; i < list.Length; i++)
                {
                    if (IsEqual(value, list.GetValue(i)))
                    {
                        index = i;
                        break;
                    }
                }

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(GetShowName(field, value), GUILayout.Width(150));
                var newindex = EditorGUILayout.Popup(index, names);
                if (newindex != index)
                {
                    field.SetValue(obj, list.GetValue(newindex));
                }

                EditorGUILayout.EndHorizontal();
            }

            return 21;
        }

        protected virtual float DrawValueDropdownFieldInspector(FieldInfo field, object obj,
            ValueDropdownAttribute valueDropdownAttribute)
        {
            object value = field.GetValue(obj);
            string showText = value?.ToString();
            if (valueDropdown.TryGetValue(field, out var list))
            {
                int index = -1;
                for (int i = 0; i < list.Length; i++)
                {
                    if (IsEqual(value, list[i].GetValue()))
                    {
                        index = i;
                        break;
                    }
                }

                if (index == -1 && list.Length > 0)
                {
                    field.SetValue(obj, list[0].GetValue());
                    index = 0;
                }

                if (index >= 0 && index < list.Length)
                {
                    showText = list[index].GetText();
                }
            }
            else
            {
                RefreshValueDropDown(field, obj, valueDropdownAttribute.ValuesGetter);
                object newValue = value;
                var itemHeight = DrawNormalField(field.FieldType, GetShowName(field, value), ref newValue, true, field);
                if (itemHeight <= 0)
                {
                    EditorGUILayout.LabelField(showText);
                }
                else if (!IsEqual(newValue, value))
                {
                    field.SetValue(obj, newValue);
                }

                return itemHeight;
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(GetShowName(field, value), GUILayout.Width(150));
            if (EditorGUILayout.DropdownButton(new GUIContent(showText), FocusType.Passive))
            {
                RefreshValueDropDown(field, obj, valueDropdownAttribute.ValuesGetter);
                if (!valueDropdown.TryGetValue(field, out list))
                {
                    Debug.LogError(valueDropdownAttribute.ValuesGetter);
                }
                else
                {
                    int index = -1;
                    for (int i = 0; i < list.Length; i++)
                    {
                        if (IsEqual(value, list[i].GetValue()))
                        {
                            index = i;
                            break;
                        }
                    }

                    var menu = new GenericMenu();
                    for (int i = 0; i < list.Length; i++)
                    {
                        var ii = i;
                        menu.AddItem(new GUIContent(list[i].GetText()), i == index, () =>
                        {
                            if (ii != index)
                            {
                                field.SetValue(obj, list[ii].GetValue());
                            }
                        });
                    }

                    menu.ShowAsContext();
                }
            }

            EditorGUILayout.EndHorizontal();
            return 21;
        }

        #endregion

        #region DrawMethod

        protected virtual float DrawMethodInspector(MethodInfo method, object obj, bool isDetails = false)
        {
            //仅支持无参方法
            if (method.GetParameters().Length != 0) return 0;
            if (method.GetCustomAttribute(typeof(ButtonAttribute)) is ButtonAttribute buttonAttribute)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(buttonAttribute.Name))
                {
                    method.Invoke(obj, null);
                }

                EditorGUILayout.EndHorizontal();
                return 21;
            }

            return 0;
        }

        #endregion

        protected virtual string GetShowNameString(MemberInfo member, out bool rename)
        {
            if (member.GetCustomAttribute(typeof(LabelTextAttribute)) is LabelTextAttribute labelTextAttribute)
            {
                rename = true;
                return labelTextAttribute.Text;
            }

            rename = false;
            return ObjectNames.NicifyVariableName(member.Name);
        }

        protected virtual GUIContent GetShowName(MemberInfo member, object value = null)
        {
            string tip = null;
            if (member.GetCustomAttribute(typeof(TooltipAttribute)) is TooltipAttribute tooltip)
            {
                tip = tooltip.tooltip;
            }

            string showname = null;
            if (member.GetCustomAttribute(typeof(LabelTextAttribute)) is LabelTextAttribute labelTextAttribute)
            {
                showname = labelTextAttribute.Text;
            }
            else
            {
                showname = ObjectNames.NicifyVariableName(member.Name);
            }

            if (value != null && member is FieldInfo fieldInfo)
            {
                var valueType = value.GetType();
                if (valueType != fieldInfo.FieldType && valueType.IsClass && !valueType.IsArray &&
                    valueType != stringType && valueType != objectType
                    && valueType != dicType && valueType != listType && !valueType.IsGenericType)
                {
                    return new GUIContent(GetShowNameString(valueType), tip ?? showname);
                }
            }

            return new GUIContent(showname, tip ?? showname);
        }

        protected virtual string GetShowNameString(Type type)
        {
            if (type.GetCustomAttribute(typeof(LabelTextAttribute)) is LabelTextAttribute labelTextAttribute)
            {
                return labelTextAttribute.Text;
            }

            return ObjectNames.NicifyVariableName(type.Name);
        }

        protected virtual GUIContent GetShowName(Type type)
        {
            string tip = null;
            if (type.GetCustomAttribute(typeof(TooltipAttribute)) is TooltipAttribute tooltip)
            {
                tip = tooltip.tooltip;
            }

            string showname = null;
            if (type.GetCustomAttribute(typeof(LabelTextAttribute)) is LabelTextAttribute labelTextAttribute)
            {
                showname = labelTextAttribute.Text;
            }
            else
            {
                showname = ObjectNames.NicifyVariableName(type.Name);
            }

            return new GUIContent(showname, tip ?? showname);
        }

        protected virtual List<Type> GetSubClassList(FieldInfo fieldInfo, object obj, Type type, out string[] names)
        {
            if (fieldInfo.GetCustomAttribute(typeof(TypeFilterAttribute)) is TypeFilterAttribute typeFilterAttribute)
            {
                temp2.Clear();
                temp3.Clear();
                RefreshValueDropDown(fieldInfo, obj, typeFilterAttribute.FilterGetter);
                if (valueDropdown.TryGetValue(fieldInfo, out var list))
                {

                    for (int i = 0; i < list.Length; i++)
                    {
                        temp2.Add(list[i].GetValue() as Type);
                        temp3.Add(list[i].GetText());
                    }

                    names = temp3.ToArray();
                    return temp2;
                }
                else
                {
                    Debug.LogError(typeFilterAttribute.FilterGetter);
                }
            }

            return TypeHelper.GetSubClassList(fieldInfo, type, out names);
        }

        protected virtual bool SelectMemberInfo(MemberInfo member, object obj, bool isDetails)
        {
            if (member is PropertyInfo prop && !prop.CanWrite) return false;
            if (member is MethodInfo && member.GetCustomAttribute(typeof(ButtonAttribute)) == null) return false;

            if (member.GetCustomAttribute(typeof(HideInInspector)) is HideInInspector)
            {
                return false;
            }

            return true;
        }

        #region Private

        private ISort[] GetSortMember(object obj)
        {
            var type = obj.GetType();
            if (sortsMap.TryGetValue(type, out var res))
            {
                return res;
            }

            sortTemp.Clear();
            groupsTemp.Clear();
            var members = type.GetMembers(BindingFlags.Public | BindingFlags.Instance);
            foreach (var member in members)
            {
                if (!SelectMemberInfo(member, obj, true)) continue;
                float sort = 0;
                if (member.GetCustomAttribute(typeof(PropertyOrderAttribute)) is PropertyOrderAttribute orderAttribute)
                {
                    sort = orderAttribute.Order;
                }

                if (member.GetCustomAttribute(typeof(BoxGroupAttribute)) is BoxGroupAttribute boxGroupAttribute)
                {
                    if (!groupsTemp.TryGetValue(boxGroupAttribute.GroupID, out var groupItem))
                    {
                        groupItem = new GroupItem()
                        {
                            MinSort = sort,
                            GroupId = boxGroupAttribute.GroupID,
                            Members = new()
                        };
                        sortTemp.Add(groupItem);
                        groupsTemp.Add(groupItem.GroupId, groupItem);
                    }

                    groupItem.Members.Add(new MemberItem() {Member = member, MinSort = sort});
                    if (sort < groupItem.MinSort)
                    {
                        groupItem.MinSort = sort;
                    }
                }
                else
                {
                    sortTemp.Add(new MemberItem() {Member = member, MinSort = sort});
                }
            }

            sortTemp.Sort(SortAb);
            foreach (var kv in groupsTemp)
            {
                kv.Value.Members.Sort(SortAb);
            }

            res = sortTemp.ToArray();
            sortsMap.Add(type, res);
            return res;
        }

        private void RefreshValueDropDown(FieldInfo field, object obj, string valuesGetter)
        {
            Type type = obj.GetType();
            string funcName = valuesGetter;
            string[] paras = null;
            object[] paraData = null;
            if (funcName.StartsWith("@"))
            {
                funcName = funcName.Replace("@", "");
                if (funcName.Contains("."))
                {
                    var vs = funcName.Split(".");
                    type = TypeHelper.FindType(vs[0]);
                    funcName = vs[1];
                }


                if (funcName.EndsWith(")"))
                {
                    if (funcName.EndsWith("()"))
                    {
                        funcName = funcName.Replace("()", "");
                    }
                    else
                    {
                        var index = funcName.IndexOf("(");
                        var paraStr = funcName.Substring(index + 1, funcName.Length - index - 2);
                        funcName = funcName.Substring(0, index);
                        paras = paraStr.Split(",");
                        paraData = new object[paras.Length];
                    }
                }
            }

            if (type != null)
            {
                if (paras != null)
                {
                    for (int i = 0; i < paras.Length; i++)
                    {
                        if (paras[i].EndsWith("()"))
                        {
                            var methodItem = obj.GetType().GetMethod(paras[i].Replace("()", ""),
                                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
                                BindingFlags.Static);
                            if (methodItem != null)
                            {
                                if (methodItem.IsStatic)
                                {
                                    paraData[i] = methodItem.Invoke(null, null);
                                }
                                else
                                {
                                    paraData[i] = methodItem.Invoke(obj, null);
                                }
                            }
                        }
                        else
                        {
                            var fieldItem = obj.GetType().GetField(paras[i],
                                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
                                BindingFlags.Static);
                            if (fieldItem != null)
                            {
                                paraData[i] = fieldItem.GetValue(obj);
                            }

                            var propItem = obj.GetType().GetProperty(paras[i],
                                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
                                BindingFlags.Static);
                            if (propItem != null && propItem.CanRead)
                            {
                                paraData[i] = propItem.GetValue(obj);
                            }
                        }
                    }
                }


                var method = type.GetMethod(funcName,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                if (method != null)
                {
                    if (!valueDropdown.TryGetValue(field, out var list) || (paraData != null && paraData.Length > 0))
                    {
                        IEnumerable data;
                        if (method.IsStatic)
                        {
                            data = method.Invoke(null, paraData) as IEnumerable;
                        }
                        else
                        {
                            data = method.Invoke(obj, paraData) as IEnumerable;
                        }

                        if (data == null) return;
                        temp.Clear();
                        foreach (var item in data)
                        {
                            if (item is IValueDropdownItem valueDropdownItem)
                            {
                                temp.Add(valueDropdownItem);
                            }
                            else
                            {
                                if (item is Type itemType &&
                                    itemType.GetCustomAttribute(typeof(LabelTextAttribute)) is LabelTextAttribute
                                        labelTextAttribute)
                                {
                                    temp.Add(new ValueDropdownItem(labelTextAttribute.Text, item));
                                }
                                else
                                {
                                    temp.Add(new ValueDropdownItem(item.ToString(), item));
                                }
                            }
                        }

                        list = temp.ToArray();
                        valueDropdown[field] = list;
                    }
                }
            }
        }

        private bool CheckShowIf(MemberInfo member, object obj, ShowIfAttribute showIf)
        {
            Type type = obj.GetType();
            string condition = showIf.Condition;
            bool isFunc = condition.EndsWith("()");
            bool isCondition = condition.StartsWith("@");
            bool onePara = !condition.Contains("&") && !condition.Contains("|");
            if (!onePara) return true; //todo:先不处理
            bool isInverse = false;
            if (isCondition)
            {
                condition = condition.Replace("@", "");
                if (condition.StartsWith("!"))
                {
                    condition = condition.Replace("!", "");
                    isInverse = true;
                }

                if (condition.Contains("."))
                {
                    var vs = condition.Split(".");
                    type = TypeHelper.FindType(vs[0]);
                    condition = vs[1];
                }

                if (condition.Contains("("))
                {
                    if (condition.Contains("()"))
                    {
                        condition = condition.Replace("()", "");
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
                if (isFunc)
                {
                    var method = type.GetMethod(condition,
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                    if (method != null)
                    {
                        bool val;
                        if (method.IsStatic)
                        {
                            val = (bool) method.Invoke(null, null);
                        }
                        else
                        {
                            val = (bool) method.Invoke(obj, null);
                        }

                        if (isInverse)
                        {
                            return !val;
                        }

                        return val;
                    }
                }
                else
                {
                    var property = type.GetProperty(condition,
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                    if (property != null && property.CanRead)
                    {
                        var val = (bool) property.GetValue(obj);
                        if (isInverse)
                        {
                            return !val;
                        }

                        return val;
                    }

                    var field2 = type.GetField(condition,
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                    if (field2 != null)
                    {
                        var val = (bool) field2.GetValue(obj);
                        if (isInverse)
                        {
                            return !val;
                        }

                        return val;
                    }
                }
            }

            return true;
        }

        private bool IsEqual(object a, object b)
        {
            if (a == null && b == null) return true;
            if (a != null && b != null)
            {
                return a.Equals(b);
            }

            return false;
        }

        private int SortAb(ISort a, ISort b)
        {
            if (a.MinSort == b.MinSort)
            {
                if (a is MemberItem ma && b is MemberItem mb)
                {
                    return ma.Member.MemberType - mb.Member.MemberType;
                }

                return 0;
            }

            return a.MinSort - b.MinSort > 0 ? 1 : -1;
        }

        #endregion
    }
}