﻿#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Nebukam.Common.Editor
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ConditionalFieldAttribute : PropertyAttribute
    {
        private readonly string _fieldToCheck;
        private readonly object _compareValue;
        private readonly bool _inverse;

        public ConditionalFieldAttribute(string fieldToCheck, object compareValue = null, bool inverse = false)
        {
            _fieldToCheck = fieldToCheck;
            _compareValue = compareValue;
            _inverse = inverse;
        }

        public bool CheckBehaviourPropertyVisible(MonoBehaviour behaviour, string propertyName)
        {
            if (string.IsNullOrEmpty(_fieldToCheck)) return true;

            var so = new SerializedObject(behaviour);
            var property = so.FindProperty(propertyName);

            return CheckPropertyVisible(property);
        }


        public bool CheckPropertyVisible(SerializedProperty property)
        {
            var conditionProperty = FindRelativeProperty(property, _fieldToCheck);
            if (conditionProperty == null) return true;

            string asString = AsStringValue(conditionProperty).ToUpper();


            if (_compareValue != null)
            {
                bool compareValueMatch = _compareValue.ToString().ToUpper() == asString;
                return compareValueMatch ? !_inverse : _inverse;
            }


            bool someValueAssigned = asString != "FALSE" && asString != "0" && asString != "NULL";
            if (someValueAssigned) return !_inverse;

            return _inverse;
        }

        private SerializedProperty FindRelativeProperty(SerializedProperty property, string toGet)
        {
            if (property.depth == 0) return property.serializedObject.FindProperty(toGet);

            var path = property.propertyPath.Replace(".Array.data[", "[");
            var elements = path.Split('.');

            var nestedProperty = NestedPropertyOrigin(property, elements);

            // if nested property is null = we hit an array property
            if (nestedProperty == null)
            {
                var cleanPath = path.Substring(0, path.IndexOf('['));
                var arrayProp = property.serializedObject.FindProperty(cleanPath);
                if (_warningsPool.Contains(arrayProp.exposedReferenceValue)) return null;
                var target = arrayProp.serializedObject.targetObject;
                var who = string.Format("Property <color=brown>{0}</color> in object <color=brown>{1}</color> caused: ", arrayProp.name,
                    target.name);

                Debug.LogWarning(who + "Array fields is not supported by [ConditionalFieldAttribute]", target);
                _warningsPool.Add(arrayProp.exposedReferenceValue);
                return null;
            }

            return nestedProperty.FindPropertyRelative(toGet);
        }

        // For [Serialized] types with [Conditional] fields
        private SerializedProperty NestedPropertyOrigin(SerializedProperty property, string[] elements)
        {
            SerializedProperty parent = null;

            for (int i = 0; i < elements.Length - 1; i++)
            {
                var element = elements[i];
                int index = -1;
                if (element.Contains("["))
                {
                    index = Convert.ToInt32(element.Substring(element.IndexOf("[", StringComparison.Ordinal))
                        .Replace("[", "").Replace("]", ""));
                    element = element.Substring(0, element.IndexOf("[", StringComparison.Ordinal));
                }

                parent = i == 0
                    ? property.serializedObject.FindProperty(element)
                    : parent.FindPropertyRelative(element);

                if (index >= 0) parent = parent.GetArrayElementAtIndex(index);
            }

            return parent;
        }


        private string AsStringValue(SerializedProperty prop)
        {
            switch (prop.propertyType)
            {
                case SerializedPropertyType.String:
                    return prop.stringValue;

                case SerializedPropertyType.Character:
                case SerializedPropertyType.Integer:
                    if (prop.type == "char") return Convert.ToChar(prop.intValue).ToString();
                    return prop.intValue.ToString();

                case SerializedPropertyType.ObjectReference:
                    return prop.objectReferenceValue != null ? prop.objectReferenceValue.ToString() : "null";

                case SerializedPropertyType.Boolean:
                    return prop.boolValue.ToString();

                case SerializedPropertyType.Enum:
                    return prop.enumNames[prop.enumValueIndex];

                default:
                    return string.Empty;
            }
        }

        //This pool is used to prevent spamming with warning messages
        //One message per property
        readonly HashSet<object> _warningsPool = new HashSet<object>();
    }

    [CustomPropertyDrawer(typeof(ConditionalFieldAttribute))]
    public class ConditionalFieldAttributeDrawer : PropertyDrawer
    {
        private ConditionalFieldAttribute Attribute
        {
            get { return _attribute ?? (_attribute = attribute as ConditionalFieldAttribute); }
        }

        private ConditionalFieldAttribute _attribute;

        private bool _toShow = true;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            _toShow = Attribute.CheckPropertyVisible(property);

            return _toShow ? EditorGUI.GetPropertyHeight(property) : 0;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (_toShow) EditorGUI.PropertyField(position, property, label, true);
        }
    }
}

#endif