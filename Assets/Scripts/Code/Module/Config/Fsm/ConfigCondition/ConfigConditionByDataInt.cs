﻿using System;
using Nino.Core;
using UnityEngine;

namespace TaoTie
{
    [NinoType(false)]
    public partial class ConfigConditionByDataInt : ConfigConditionByData<int>
    {

        public override bool Equals(ConfigCondition other)
        {
            if (other is ConfigConditionByDataInt data)
            {
                return Key == data.Key && Value == data.Value && Mode == data.Mode;
            }

            return false;
        }
        public override ConfigCondition Copy()
        {
            return new ConfigConditionByDataInt()
            {
                Key = Key,
                Value = Value,
                Mode = Mode
            };
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