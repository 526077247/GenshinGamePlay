using System;
using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [LabelText("当添加或附加组之后-组localId判断")]
    [TriggerType(typeof(ConfigGroupLoadGearTrigger))]
    [NinoSerialize]
    public class ConfigGroupLoadLocalIdCondition : ConfigGearCondition<GroupLoadEvent>
    {
        [Tooltip(GearTooltips.CompareMode)] [OnValueChanged("@CheckModeType(value,mode)")]
        [NinoMember(1)]
        public CompareMode mode;
        [ValueDropdown("@OdinDropdownHelper.GetGearGroupIds()")]
        [NinoMember(2)]
        public int value;
        
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