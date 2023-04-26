using System;
using Nino.Serialization;
using UnityEngine;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigConditionByDataFloat : ConfigCondition
    {
        public string Key;
        public float Value;
        public CompareMode Mode;
        
        public ConfigConditionByDataFloat(){}
        public ConfigConditionByDataFloat(string key, float val, CompareMode mode)
        {
            this.Key = key;
            this.Value = val;
            this.Mode = mode;
        }

        public ConfigConditionByDataFloat(ConfigConditionByDataFloat other)
        {
            this.Key = other.Key;
            this.Value = other.Value;
            this.Mode = other.Mode;
        }

        public override ConfigCondition Copy()
        {
            return new ConfigConditionByDataFloat(this);
        }

        public override bool IsMatch(Fsm fsm)
        {
            float val = fsm.Component.GetFloat(this.Key);
            switch (this.Mode)
            {
                case CompareMode.Equal:
                    return val == this.Value;
                case CompareMode.NotEqual:
                    return val != this.Value;
                case CompareMode.Greater:
                    return val > this.Value;
                case CompareMode.Less:
                    return val < this.Value;
                case CompareMode.LEqual:
                    return val <= this.Value;
                case CompareMode.GEqual:
                    return val >= this.Value;
                default:
                    break;
            }
            return false;
        }
    }
}