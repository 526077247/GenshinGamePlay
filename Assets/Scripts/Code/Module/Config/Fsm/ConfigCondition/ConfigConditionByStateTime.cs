using System;
using Nino.Serialization;
using UnityEngine;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigConditionByStateTime : ConfigCondition
    {
        public float Time;
        public bool IsNormalized;
        public CompareMode Mode;

        public ConfigConditionByStateTime(){}
        public ConfigConditionByStateTime(float time, bool isNormalized, CompareMode mode)
        {
            this.Time = time;
            this.IsNormalized = isNormalized;
            this.Mode = mode;
        }

        public ConfigConditionByStateTime(ConfigConditionByStateTime other)
        {
            this.Time = other.Time;
            this.IsNormalized = other.IsNormalized;
            this.Mode = other.Mode;
        }

        public override ConfigCondition Copy()
        {
            return new ConfigConditionByStateTime(this);
        }

        public override bool IsMatch(Fsm fsm)
        {
            float val;
            if (this.IsNormalized)
                val = fsm.StateNormalizedTime;
            else
                val = fsm.StateTime;

            switch (this.Mode)
            {
                case CompareMode.Equal:
                    return val == this.Time;
                case CompareMode.NotEqual:
                    return val != this.Time;
                case CompareMode.Greater:
                    return val > this.Time;
                case CompareMode.Less:
                    return val < this.Time;
                case CompareMode.LEqual:
                    return val <= this.Time;
                case CompareMode.GEqual:
                    return val >= this.Time;
                default:
                    break;
            }
            return false;
        }
    }
}