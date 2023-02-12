using System;
using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ComplexVariableValue : AbstractVariableValue
    {
        [NinoMember(1)][LabelText("左值")]
        public AbstractVariableValue value1;

        [NinoMember(2)][Tooltip("操作类型")]
        public LogicMode _op;
        
        [InfoBox("注意：除数不能为0")] [ShowIf("@_op != LogicMode.Default")]
        [NinoMember(3)][LabelText("右值")]
        public AbstractVariableValue value2;
        

        public override float Resolve(IEventBase obj, VariableSet set)
        {
            float v1 = value1.Resolve(obj, set), v2 = value2.Resolve(obj, set);
            return GetLogicValue(v1, v2, _op);
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