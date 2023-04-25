using System;
using Nino.Serialization;
using UnityEngine;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigConditionByStateTime : ConfigCondition
    {
        public float time;
        public bool isNormalized;
        public CompareMode mode;

        public ConfigConditionByStateTime(){}
        public ConfigConditionByStateTime(float time, bool isNormalized, CompareMode mode)
        {
            this.time = time;
            this.isNormalized = isNormalized;
            this.mode = mode;
        }

        public ConfigConditionByStateTime(ConfigConditionByStateTime other)
        {
            this.time = other.time;
            this.isNormalized = other.isNormalized;
            this.mode = other.mode;
        }

        public override ConfigCondition Copy()
        {
            return new ConfigConditionByStateTime(this);
        }

        public override bool IsMatch(Fsm fsm)
        {
            float val;
            if (this.isNormalized)
                val = fsm.stateNormalizedTime;
            else
                val = fsm.stateTime;

            switch (this.mode)
            {
                case CompareMode.Equal:
                    return val == this.time;
                case CompareMode.NotEqual:
                    return val != this.time;
                case CompareMode.Greater:
                    return val > this.time;
                case CompareMode.Less:
                    return val < this.time;
                case CompareMode.LEqual:
                    return val <= this.time;
                case CompareMode.GEqual:
                    return val >= this.time;
                default:
                    break;
            }
            return false;
        }
    }
}