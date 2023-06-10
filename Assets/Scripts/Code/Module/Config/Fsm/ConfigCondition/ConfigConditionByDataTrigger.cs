using System;
using Nino.Serialization;
using UnityEngine;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigConditionByDataTrigger : ConfigCondition
    {
        [NinoMember(1)]
        public string Key;

        public override ConfigCondition Copy()
        {
            return new ConfigConditionByDataTrigger() {Key = this.Key};
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