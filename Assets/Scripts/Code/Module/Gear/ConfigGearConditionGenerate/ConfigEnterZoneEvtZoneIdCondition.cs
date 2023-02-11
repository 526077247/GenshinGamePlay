using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [Serializable][LabelText("进入触发区域的id")]
    [TriggerType(typeof(ConfigEnterZoneGearTrigger))]
    public class ConfigEnterZoneEvtZoneIdCondition : ConfigGearCondition<EnterZoneEvent>
    {
        [Tooltip(GearTooltips.CompareMode)] [OnValueChanged("@CheckModeType(value,mode)")] [SerializeField]
        public CompareMode mode;
        [ValueDropdown("@OdinDropdownHelper.GetGearZoneIds()")]
        [SerializeField] public int value;

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