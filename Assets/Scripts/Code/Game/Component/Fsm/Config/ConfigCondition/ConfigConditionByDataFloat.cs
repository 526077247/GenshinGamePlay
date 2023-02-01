using System;
using UnityEngine;

namespace TaoTie
{

    public class ConfigConditionByDataFloat : ConfigCondition
    {
        public string key;
        public float value;
        public CompareMode mode;
        
        public ConfigConditionByDataFloat(string key, float val, CompareMode mode)
        {
            this.key = key;
            this.value = val;
            this.mode = mode;
        }

        public ConfigConditionByDataFloat(ConfigConditionByDataFloat other)
        {
            this.key = other.key;
            this.value = other.value;
            this.mode = other.mode;
        }

        public override ConfigCondition Copy()
        {
            return new ConfigConditionByDataFloat(this);
        }

        public override bool IsMatch(Fsm fsm)
        {
            float val = fsm.Component.GetFloat(this.key);
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