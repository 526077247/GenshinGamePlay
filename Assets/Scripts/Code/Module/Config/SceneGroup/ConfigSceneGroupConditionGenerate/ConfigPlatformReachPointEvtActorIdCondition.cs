using System;
using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [TriggerType(typeof(ConfigPlatformReachPointEvtTrigger))]
    [NinoType(false)]
    [LabelText("抵达寻路点ActorId")]
    public partial class ConfigPlatformReachPointEvtActorIdCondition : ConfigSceneGroupCondition<PlatformReachPointEvt>
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
        [ValueDropdown("@"+nameof(OdinDropdownHelper)+"."+nameof(OdinDropdownHelper.GetSceneGroupActorIds)+"()",AppendNextDrawer = true)]
        [LabelText("单位Id")]
#endif
        public Int32 Value;

        public override bool IsMatch(PlatformReachPointEvt obj, SceneGroup sceneGroup)
        {
            return IsMatch(Value, obj.ActorId, Mode);
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
