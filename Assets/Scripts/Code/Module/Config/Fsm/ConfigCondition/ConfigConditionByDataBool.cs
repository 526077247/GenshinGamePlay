using System;
using Nino.Serialization;
using UnityEngine;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigConditionByDataBool : ConfigCondition
    {
        public string Key;
        public bool Value;
        public CompareMode Mode;
        
        public ConfigConditionByDataBool(){}
        public ConfigConditionByDataBool(string key, bool val, CompareMode mode)
        {
            this.Key = key;
            this.Value = val;
            this.Mode = mode;
        }

        public ConfigConditionByDataBool(ConfigConditionByDataBool other)
        {
            this.Key = other.Key;
            this.Value = other.Value;
            this.Mode = other.Mode;
        }

        public override ConfigCondition Copy()
        {
            return new ConfigConditionByDataBool(this);
        }

        public override bool IsMatch(Fsm fsm)
        {
            bool val = fsm.Component.GetBool(this.Key);
            switch (this.Mode)
            {
                case CompareMode.Equal:
                    return val == this.Value;
                case CompareMode.NotEqual:
                    return val != this.Value;
            }
            return false;
        }
    }
}