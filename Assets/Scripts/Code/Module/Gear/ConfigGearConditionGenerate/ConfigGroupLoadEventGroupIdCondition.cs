using System;
using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [TriggerType(typeof(ConfigGroupLoadEventTrigger))]
    [NinoSerialize]
    public class ConfigGroupLoadEventGroupIdCondition : ConfigGearCondition<GroupLoadEvent>
    {
        [Tooltip(GearTooltips.CompareMode)] [OnValueChanged("@CheckModeType(value,mode)")] 
        [NinoMember(1)]
        public CompareMode mode;
        [NinoMember(2)]
        [ValueDropdown("@OdinDropdownHelper.GetGearGroupIds()")]
        public Int32 value;

        public override bool IsMatch(GroupLoadEvent obj,Gear gear)
        {
            return IsMatch(value, obj.GroupId, mode);
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
