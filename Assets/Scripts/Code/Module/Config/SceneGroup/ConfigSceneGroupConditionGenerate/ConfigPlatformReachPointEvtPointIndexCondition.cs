using System;
using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [TriggerType(typeof(ConfigPlatformReachPointEvtTrigger))]
    [NinoType(false)]
    [LabelText("抵达的寻路点")]
    public partial class ConfigPlatformReachPointEvtPointIndexCondition : ConfigSceneGroupCondition<PlatformReachPointEvt>
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

        public override bool IsMatch(PlatformReachPointEvt obj, SceneGroup sceneGroup)
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
