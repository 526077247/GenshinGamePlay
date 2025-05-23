using System;
using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [TriggerType(typeof(ConfigAvatarNearPlatformEvtTrigger))]
    [NinoType(false)]
    [LabelText("靠近单位的当前寻路点或下一个点的序号")]
    public partial class ConfigAvatarNearPlatformEvtPointIndexCondition : ConfigSceneGroupCondition<AvatarNearPlatformEvt>
    {
        [Tooltip(SceneGroupTooltips.CompareMode)]
#if UNITY_EDITOR
        [OnValueChanged("@"+nameof(CheckModeType)+"("+nameof(Value)+","+nameof(Mode)+")")]
#endif
        [NinoMember(1)]
        [LabelText("判断类型")]
        public CompareMode Mode;
        [NinoMember(2)]
        public Int32 Value;

        public override bool IsMatch(AvatarNearPlatformEvt obj, SceneGroup sceneGroup)
        {
            return IsMatch(Value, obj.PointIndex, Mode);
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
