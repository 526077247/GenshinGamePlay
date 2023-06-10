using System;
using Nino.Serialization;
using UnityEngine;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigConditionByDataBool : ConfigCondition
    {
        [NinoMember(1)]
        public string Key;
        [NinoMember(2)]
        public bool Value;
        [NinoMember(3)]
        public CompareMode Mode;

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