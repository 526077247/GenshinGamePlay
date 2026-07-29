using System;
using ProtoBuf;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [ProtoContract]
    public partial class OperatorSceneGroupValue : BaseSceneGroupValue
    {
        [ProtoMember(1)][LabelText("左值")]
        public BaseSceneGroupValue Value1;

        [ProtoMember(2)][LabelText("操作类型")]
        public LogicMode Op;
        
        [InfoBox("注意：除数不能为0")]
        [ProtoMember(3)][LabelText("右值")]
        public BaseSceneGroupValue Value2;
        

        public override float Resolve(IEventBase obj, DynDictionary set)
        {
            float v1 = Value1.Resolve(obj, set), v2 = Value2.Resolve(obj, set);
            return GetLogicValue(v1, v2, Op);
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