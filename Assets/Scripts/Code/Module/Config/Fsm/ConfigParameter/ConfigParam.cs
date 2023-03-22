using System;
using UnityEngine;

namespace TaoTie
{
    public abstract class ConfigParam
    {
        public string key;
        public bool needSyncAnimator;

        public void SetValue(DynDictionary dynDictionary, float val)
        {
            
            dynDictionary.Set(this.key, val);

        }

        public void SetValue(DynDictionary dynDictionary, int val)
        {
            dynDictionary.Set(this.key, val);
        }

        public void SetValue(DynDictionary dynDictionary, bool val)
        {
            dynDictionary.Set(this.key, val?1:0);
        }

        public float GetFloat(DynDictionary dynDictionary)
        {
            return dynDictionary.Get(this.key);
        }

        public int GetInt(DynDictionary dynDictionary)
        {
            return (int)dynDictionary.Get(this.key);
        }

        public bool GetBool(DynDictionary dynDictionary)
        {
            return dynDictionary.Get(this.key) != 0;
        }

        public abstract void SetDefaultValue(DynDictionary dynDictionary);
    }
    
    public abstract class ConfigParam<T> : ConfigParam
    {
        public T defaultValue;
        public ConfigParam(){}
        public ConfigParam(string key, T defaultVal, bool needSyncAnimator)
        {
            base.key = key;
            this.defaultValue = defaultVal;
            this.needSyncAnimator = needSyncAnimator;
        }
    }
}