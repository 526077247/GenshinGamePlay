using System;
using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [LabelText("进入触发区域的id")]
    [TriggerType(typeof(ConfigExitZoneGearTrigger))]
    [NinoSerialize]
    public class ConfigExitZoneEvtZoneIdCondition : ConfigGearCondition<ExitZoneEvent>
    {
        [NinoMember(1)]
        [Tooltip(GearTooltips.CompareMode)] [OnValueChanged("@CheckModeType(value,mode)")]
        public CompareMode mode;
        [NinoMember(2)]
        [ValueDropdown("@OdinDropdownHelper.GetGearZoneIds()")]
        public int value;

        public override bool IsMatch(ExitZoneEvent obj,Gear gear)
        {
            return IsMatch(value, obj.ZoneLocalId, mode);
        }
#if UNITY_EDITOR
        protected override bool CheckModeType<T>(T t, CompareMode mode)
        {
            if (!base.CheckModeType(t, mode))
            {
                mode = CompareMode.Equal;
                return false;
            }

            return true;
        }
#endif
    }
}