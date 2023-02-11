using System;
using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [TriggerType(typeof(ConfigEnterZoneEventTrigger))]
    [NinoSerialize]
    public class ConfigEnterZoneEventZoneLocalIdCondition : ConfigGearCondition<EnterZoneEvent>
    {
        [Tooltip(GearTooltips.CompareMode)] [OnValueChanged("@CheckModeType(value,mode)")] 
        [NinoMember(1)]
        public CompareMode mode;
        [NinoMember(2)]
        [ValueDropdown("@OdinDropdownHelper.GetGearZoneIds()")]
        public Int64 value;

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
