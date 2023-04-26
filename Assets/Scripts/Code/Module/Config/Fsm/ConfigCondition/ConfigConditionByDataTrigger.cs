using System;
using Nino.Serialization;
using UnityEngine;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigConditionByDataTrigger : ConfigCondition
    {
        public string Key;

        public ConfigConditionByDataTrigger(){}
        public ConfigConditionByDataTrigger(string key)
        {
            this.Key = key;
        }

        public ConfigConditionByDataTrigger(ConfigConditionByDataTrigger other)
        {
            this.Key = other.Key;
        }

        public override ConfigCondition Copy()
        {
            return new ConfigConditionByDataTrigger(this);
        }

        public override bool IsMatch(Fsm fsm)
        {
            bool val = fsm.Component.GetBool(this.Key);
            return val;
        }

        public override void OnTransitionApply(Fsm fsm)
        {
            fsm.Component.SetData(this.Key, false);
        }
    }
}