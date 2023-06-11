using System;
using Nino.Serialization;
using UnityEngine;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigConditionByDataFloat : ConfigConditionByData<float>
    {

        public override bool Equals(ConfigCondition other)
        {
            if (other is ConfigConditionByDataFloat data)
            {
                return Key == data.Key && Value == data.Value && Mode == data.Mode;
            }

            return false;
        }
        public override ConfigCondition Copy()
        {
            return new ConfigConditionByDataFloat()
            {
                Key = Key,
                Value = Value,
                Mode = Mode
            };
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