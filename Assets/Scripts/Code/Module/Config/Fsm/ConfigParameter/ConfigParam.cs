using System;
using Nino.Serialization;
using UnityEngine;

namespace TaoTie
{
    [NinoSerialize]
    public abstract partial class ConfigParam
    {
        public string Key;
        public bool NeedSyncAnimator;

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
    [NinoSerialize]
    public abstract class ConfigParam<T> : ConfigParam
    {
        public T defaultValue;
        public ConfigParam(){}
        public ConfigParam(string key, T defaultVal, bool needSyncAnimator)
        {
            base.Key = key;
            this.defaultValue = defaultVal;
            this.NeedSyncAnimator = needSyncAnimator;
        }
    }
}