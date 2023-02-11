using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [Serializable][LabelText("当添加或附加组之后-组localId判断")]
    [TriggerType(typeof(ConfigGroupLoadGearTrigger))]
    public class ConfigGroupLoadLocalIdCondition : ConfigGearCondition
    {
        [Tooltip(GearTooltips.CompareMode)] [OnValueChanged("@CheckModeType(value,mode)")] [SerializeField]
        public CompareMode mode;
        [ValueDropdown("@OdinDropdownHelper.GetGearGroupIds()")]
        [SerializeField]public int value;
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