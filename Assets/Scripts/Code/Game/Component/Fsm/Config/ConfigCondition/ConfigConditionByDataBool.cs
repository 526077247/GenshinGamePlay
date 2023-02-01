using System;
using UnityEngine;

namespace TaoTie
{

    public class ConfigConditionByDataBool : ConfigCondition
    {
        public string key;
        public bool value;
        public CompareMode mode;
        

        public ConfigConditionByDataBool(string key, bool val, CompareMode mode)
        {
            this.key = key;
            this.value = val;
            this.mode = mode;
        }

        public ConfigConditionByDataBool(ConfigConditionByDataBool other)
        {
            this.key = other.key;
            this.value = other.value;
            this.mode = other.mode;
        }

        public override ConfigCondition Copy()
        {
            return new ConfigConditionByDataBool(this);
        }

        public override bool IsMatch(Fsm fsm)
        {
            bool val = fsm.Component.GetBool(this.key);
            switch (this.mode)
            {
                case CompareMode.Equal:
                    return val == this.value;
                case CompareMode.NotEqual:
                    return val != this.value;
            }
            return false;
        }
    }
}