using System;
using Nino.Serialization;
using UnityEngine;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigConditionByDataInt : ConfigCondition
    {
        public string key;
        public int value;
        public CompareMode mode;
        
        public ConfigConditionByDataInt(){}
        public ConfigConditionByDataInt(string key, int val, CompareMode mode)
        {
            this.key = key;
            this.value = val;
            this.mode = mode;
        }

        public ConfigConditionByDataInt(ConfigConditionByDataInt other)
        {
            this.key = other.key;
            this.value = other.value;
            this.mode = other.mode;
        }

        public override ConfigCondition Copy()
        {
            return new ConfigConditionByDataInt(this);
        }

        public override bool IsMatch(Fsm fsm)
        {
            int val = fsm.Component.GetInt(this.key);
            switch (this.mode)
            {
                case CompareMode.Equal:
                    return val == this.value;
                case CompareMode.NotEqual:
                    return val != this.value;
                case CompareMode.Greater:
                    return val > this.value;
                case CompareMode.Less:
                    return val < this.value;
                case CompareMode.LEqual:
                    return val <= this.value;
                case CompareMode.GEqual:
                    return val >= this.value;
                default:
                    break;
            }
            return false;
        }
    }
}