using System;
using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [LabelText("进入触发区域的id")]
    [TriggerType(typeof(ConfigEnterZoneGearTrigger))]
    [NinoSerialize]
    public class ConfigEnterZoneEvtZoneIdCondition : ConfigGearCondition<EnterZoneEvent>
    {
        [Tooltip(GearTooltips.CompareMode)] [OnValueChanged("@CheckModeType(value,mode)")] 
        [NinoMember(1)]
        public CompareMode mode;
        [NinoMember(2)]
        [ValueDropdown("@OdinDropdownHelper.GetGearZoneIds()")]
        public int value;

        public override bool IsMatch(EnterZoneEvent obj,Gear gear)
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