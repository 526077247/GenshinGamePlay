using System;
using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [TriggerType(typeof(ConfigEnterZoneEventTrigger))]
    [NinoType(false)]
    [LabelText("区域Id")]
    public partial class ConfigEnterZoneEventZoneLocalIdCondition : ConfigSceneGroupCondition<EnterZoneEvent>
    {
        [Tooltip(SceneGroupTooltips.CompareMode)]
#if UNITY_EDITOR
        [OnValueChanged("@"+nameof(CheckModeType)+"("+nameof(Value)+","+nameof(Mode)+")")]
#endif
        [NinoMember(1)]
        [LabelText("判断类型")]
        public CompareMode Mode;
        [NinoMember(2)]
#if UNITY_EDITOR
        [ValueDropdown("@"+nameof(OdinDropdownHelper)+"."+nameof(OdinDropdownHelper.GetSceneGroupZoneIds)+"()",AppendNextDrawer = true)]
        [LabelText("区域Id")]
#endif
        public Int32 Value;

        public override bool IsMatch(EnterZoneEvent obj, SceneGroup sceneGroup)
        {
            return IsMatch(Value, obj.ZoneLocalId, Mode);
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
