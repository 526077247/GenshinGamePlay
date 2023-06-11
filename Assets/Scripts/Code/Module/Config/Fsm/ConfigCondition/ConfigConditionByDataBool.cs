using System;
using Nino.Serialization;
using UnityEngine;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigConditionByDataBool : ConfigConditionByData<bool>
    {
        public override bool Equals(ConfigCondition other)
        {
            if (other is ConfigConditionByDataBool data)
            {
                return Key == data.Key && Value == data.Value && Mode == data.Mode;
            }

            return false;
        }

        public override ConfigCondition Copy()
        {
            return new ConfigConditionByDataBool()
            {
                Key = this.Key,
                Value = this.Value,
                Mode = this.Mode
            };
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