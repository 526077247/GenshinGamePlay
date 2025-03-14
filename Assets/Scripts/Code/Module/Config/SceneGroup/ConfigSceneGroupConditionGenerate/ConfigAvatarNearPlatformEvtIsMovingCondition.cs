using System;
using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [TriggerType(typeof(ConfigAvatarNearPlatformEvtTrigger))]
    [NinoType(false)]
    [LabelText("靠近单位的是否正在移动")]
    public partial class ConfigAvatarNearPlatformEvtIsMovingCondition : ConfigSceneGroupCondition<AvatarNearPlatformEvt>
    {
        [Tooltip(SceneGroupTooltips.CompareMode)]
#if UNITY_EDITOR
        [OnValueChanged("@"+nameof(CheckModeType)+"("+nameof(Value)+","+nameof(Mode)+")")]
#endif
        [NinoMember(1)]
        [LabelText("判断类型")]
        public CompareMode Mode;
        [NinoMember(2)]
        public Boolean Value;

        public override bool IsMatch(AvatarNearPlatformEvt obj, SceneGroup sceneGroup)
        {
            return IsMatch(Value, obj.IsMoving, Mode);
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
