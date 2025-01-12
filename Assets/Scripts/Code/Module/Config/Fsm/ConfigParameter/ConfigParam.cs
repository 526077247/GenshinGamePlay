using System;
using Nino.Core;
using UnityEngine;

namespace TaoTie
{
    [NinoType(false)]
    public abstract partial class ConfigParam
    {
        [NinoMember(1)]
        public string Key;
        [NinoMember(2)]
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
    [NinoType(false)]
    public abstract class ConfigParam<T> : ConfigParam
    {
        [NinoMember(3)]
        public T defaultValue;
    }
}