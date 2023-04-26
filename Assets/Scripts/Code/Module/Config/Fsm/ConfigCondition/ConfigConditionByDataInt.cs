using System;
using Nino.Serialization;
using UnityEngine;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigConditionByDataInt : ConfigCondition
    {
        public string Key;
        public int Value;
        public CompareMode Mode;
        
        public ConfigConditionByDataInt(){}
        public ConfigConditionByDataInt(string key, int val, CompareMode mode)
        {
            this.Key = key;
            this.Value = val;
            this.Mode = mode;
        }

        public ConfigConditionByDataInt(ConfigConditionByDataInt other)
        {
            this.Key = other.Key;
            this.Value = other.Value;
            this.Mode = other.Mode;
        }

        public override ConfigCondition Copy()
        {
            return new ConfigConditionByDataInt(this);
        }

        public override bool IsMatch(Fsm fsm)
        {
            int val = fsm.Component.GetInt(this.Key);
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