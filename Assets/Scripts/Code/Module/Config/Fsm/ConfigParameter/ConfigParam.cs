using System;
using ProtoBuf;
using UnityEngine;

namespace TaoTie
{
    [ProtoContract]
    [ProtoInclude(100, typeof(ConfigParam<bool>))]
    [ProtoInclude(101, typeof(ConfigParam<float>))]
    [ProtoInclude(102, typeof(ConfigParam<int>))]
    public abstract partial class ConfigParam
    {
        [ProtoMember(1)][NotNull]
        public string Key;
        [ProtoMember(2)]
        public bool NeedSyncAnimator;
        [ProtoMember(3)]
        public AnimatorFsmType ParameterType;

        public void SetValue(DynDictionary dynDictionary, float val)
        {
            
            dynDictionary.Set(this.Key, val);

        }

        public void SetValue(DynDictionary dynDictionary, int val)
        {
            dynDictionary.Set(this.Key, val);
        }

        public void SetValue(DynDictionary dynDictionary, bool val)
        {
            dynDictionary.Set(this.Key, val?1:0);
        }

        public float GetFloat(DynDictionary dynDictionary)
        {
            return dynDictionary.Get(this.Key);
        }

        public int GetInt(DynDictionary dynDictionary)
        {
            return (int)dynDictionary.Get(this.Key);
        }

        public bool GetBool(DynDictionary dynDictionary)
        {
            return dynDictionary.Get(this.Key) != 0;
        }

        public abstract void SetDefaultValue(DynDictionary dynDictionary);
    }
    [ProtoContract]
    [ProtoInclude(100, typeof(ConfigParamInt))]
    [ProtoInclude(101, typeof(ConfigParamFloat))]
    [ProtoInclude(102, typeof(ConfigParamBool))]
    [ProtoInclude(103, typeof(ConfigParamTrigger))]
    public abstract class ConfigParam<T> : ConfigParam
    {
        [ProtoMember(3)]
        public T defaultValue;
    }
}