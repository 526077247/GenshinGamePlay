﻿using System;
using Nino.Core;
using UnityEngine;

namespace TaoTie
{
    [NinoType(false)]
    public partial class ConfigConditionByDataTrigger : ConfigCondition
    {
        [NinoMember(1)][NotNull]
#if UNITY_EDITOR
        [Sirenix.OdinInspector.ValueDropdown("@"+nameof(OdinDropdownHelper)+"."+nameof(OdinDropdownHelper.GetFSMConstKey)+"()", AppendNextDrawer = true)]
#endif
        public string Key;

        public override bool Equals(ConfigCondition other)
        {
            if (other is ConfigConditionByDataTrigger data)
            {
                return Key == data.Key;
            }

            return false;
        }
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