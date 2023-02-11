using System;

using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TaoTie
{
    // Condition
    public abstract class ConfigGearCondition
    {
        public abstract bool IsMatch(IEventBase obj, Gear gear);
        
        public enum CompareMode
        {
            [LabelText("==")] Equal,
            [LabelText("!=")] NotEqual,
            [LabelText(">")] Greater,
            [LabelText("<")] Less,
            [LabelText("<=")] LEqual,
            [LabelText(">=")] GEqual,
        }

        public bool IsMatch(int configValue, int evtValue, CompareMode mode)
        {
            switch (mode)
            {
                case CompareMode.Equal:
                    return evtValue == configValue;
                case CompareMode.NotEqual:
                    return evtValue != configValue;
                case CompareMode.Greater:
                    return evtValue > configValue;
                case CompareMode.Less:
                    return evtValue < configValue;
                case CompareMode.LEqual:
                    return evtValue <= configValue;
                case CompareMode.GEqual:
                    return evtValue >= configValue;
                default:
                    return false;
            }
        }

        public bool IsMatch(float configValue, float evtValue, CompareMode mode)
        {
            switch (mode)
            {
                case CompareMode.Equal:
                    return evtValue == configValue;
                case CompareMode.NotEqual:
                    return evtValue != configValue;
                case CompareMode.Greater:
                    return evtValue > configValue;
                case CompareMode.Less:
                    return evtValue < configValue;
                case CompareMode.LEqual:
                    return evtValue <= configValue;
                case CompareMode.GEqual:
                    return evtValue >= configValue;
                default:
                    return false;
            }
        }

        public bool IsMatch(string configValue, string evtValue, CompareMode mode)
        {
            switch (mode)
            {
                case CompareMode.Equal:
                    return evtValue == configValue;
                case CompareMode.NotEqual:
                    return evtValue != configValue;
                default:
                    Log.Error("string类型不支持" + mode);
                    return false;
            }
        }

        protected bool IsMatch(bool configValue, bool evtValue, CompareMode mode)
        {
            switch (mode)
            {
                case CompareMode.Equal:
                    return evtValue == configValue;
                case CompareMode.NotEqual:
                    return evtValue != configValue;
                default:
                    Log.Error("bool类型不支持" + mode);
                    return false;
            }
        }


        public enum LogicMode
        {
            [LabelText("无")] Default,
            [LabelText("+")] Add,
            [LabelText("-")] Red,
            [LabelText("*")] Mul,
            [LabelText("÷")] Div,
            [LabelText("%")] Rem,
            [LabelText("^")] Pow,
        }

        public int GetLogicValue(int from, int value, LogicMode mode)
        {
            switch (mode)
            {
                case LogicMode.Add:
                    return from + value;
                case LogicMode.Red:
                    return from - value;
                case LogicMode.Mul:
                    return from * value;
                case LogicMode.Div:
                    if (value == 0) return from;
                    return from / value;
                case LogicMode.Rem:
                    if (value == 0) return from;
                    return from % value;
                case LogicMode.Pow:
                    return (int)Mathf.Pow(from,value);
                case LogicMode.Default:
                    return from;
                default:
                    Log.Error("类型不支持" + mode);
                    return from;
            }
        }
        public float GetLogicValue(float from, float value, LogicMode mode)
        {
            switch (mode)
            {
                case LogicMode.Add:
                    return from + value;
                case LogicMode.Red:
                    return from - value;
                case LogicMode.Mul:
                    return from * value;
                case LogicMode.Div:
                    if (value == 0) return from;
                    return from / value;
                case LogicMode.Rem:
                    if (value == 0) return from;
                    return from % value;
                case LogicMode.Pow:
                    return (int)Mathf.Pow(from,value);
                default:
                    Log.Error("类型不支持" + mode);
                    return from;
            }
        }

#if UNITY_EDITOR
        [Obsolete] [LabelText("策划备注")] [PropertyOrder(int.MinValue + 1)]
        public string remarks;

        public static void ShowNotification(string tips)
        {
            var game = EditorWindow.GetWindow(typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView"));
            game?.ShowNotification(new GUIContent($"{tips}"));
        }

        
        protected virtual bool CheckModeType<T>(T t, CompareMode mode)
        {
            Type type = TypeInfo<T>.Type;
            if (type == TypeInfo<int>.Type || type == TypeInfo<short>.Type || type == TypeInfo<long>.Type ||
                type == TypeInfo<byte>.Type || type == TypeInfo<uint>.Type || type == TypeInfo<ushort>.Type ||
                type == TypeInfo<ulong>.Type || type == TypeInfo<sbyte>.Type || type == TypeInfo<float>.Type ||
                type == TypeInfo<double>.Type || type == TypeInfo<decimal>.Type)
            {
                return true;
            }

            if (mode != CompareMode.Equal && mode != CompareMode.NotEqual)
            {
                var str = type.Name + "类型不支持CompareMode." + mode;
                Log.Error(str);
                ShowNotification(str);
                return false;
            }

            return true;
        }

#endif
    }
    
    public abstract class ConfigGearCondition<T>:ConfigGearCondition where T:IEventBase
    {
        private Type EventType => TypeInfo<T>.Type;
        public sealed override bool IsMatch(IEventBase obj, Gear gear)
        {
            if (EventType != obj.GetType()) return false;
            return IsMatch((T)obj, gear);
        }
        
        public abstract bool IsMatch(T obj, Gear gear);
    }
}