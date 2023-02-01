using System;
using UnityEngine;

namespace TaoTie
{

    public class ConfigConditionByDataTrigger : ConfigCondition
    {
        public string key;

        public ConfigConditionByDataTrigger(string key)
        {
            this.key = key;
        }

        public ConfigConditionByDataTrigger(ConfigConditionByDataTrigger other)
        {
            this.key = other.key;
        }

        public override ConfigCondition Copy()
        {
            return new ConfigConditionByDataTrigger(this);
        }

        public override bool IsMatch(Fsm fsm)
        {
            bool val = fsm.Component.GetBool(this.key);
            return val;
        }

        public override void OnTransitionApply(Fsm fsm)
        {
            fsm.Component.SetData(this.key, false);
        }
    }
}