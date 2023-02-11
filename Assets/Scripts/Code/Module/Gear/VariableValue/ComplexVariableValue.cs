using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [Serializable]
    public class ComplexVariableValue : AbstractVariableValue
    {
        [SerializeField, SerializeReference][LabelText("左值")]
        public AbstractVariableValue value1;

        [Tooltip("操作类型")] [SerializeField]
        public LogicMode _op;
        
        [InfoBox("注意：除数不能为0")] [ShowIf("@_op != LogicMode.Default")]
        [SerializeField, SerializeReference][LabelText("右值")]
        public AbstractVariableValue value2;
        

        public override float Resolve(IEventBase obj, VariableSet set)
        {
            float v1 = value1.Resolve(obj, set), v2 = value2.Resolve(obj, set);
            return GetLogicValue(v1, v2, _op);
        }
        public enum LogicMode
        {
            [LabelText("无")] Default,
            [LabelText("加")] Add,
            [LabelText("减")] Red,
            [LabelText("乘")] Mul,
            [LabelText("除")] Div,
            [LabelText("取余")] Rem,
            [LabelText("次方")] Pow,
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
                    return (int) Mathf.Pow(from, value);
                case LogicMode.Default:
                    return from;
                default:
                    Log.Error("类型不支持" + mode);
                    return from;
            }
        }
    }
}