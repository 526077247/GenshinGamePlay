using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [Serializable][LabelText("进入触发区域的id")]
    [TriggerType(typeof(ConfigExitZoneGearTrigger))]
    public class ConfigExitZoneEvtZoneIdCondition : ConfigGearCondition
    {
        [Tooltip(GearTooltips.CompareMode)] [OnValueChanged("@CheckModeType(value,mode)")] [SerializeField]
        public CompareMode mode;
        [ValueDropdown("@OdinDropdownHelper.GetGearZoneIds()")]
        [SerializeField] public int value;

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