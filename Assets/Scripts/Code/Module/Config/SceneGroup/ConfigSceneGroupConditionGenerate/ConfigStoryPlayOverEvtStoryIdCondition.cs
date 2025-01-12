using System;
using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [TriggerType(typeof(ConfigStoryPlayOverEvtTrigger))]
    [NinoType(false)]
    public partial class ConfigStoryPlayOverEvtStoryIdCondition : ConfigSceneGroupCondition<StoryPlayOverEvt>
    {
        [Tooltip(SceneGroupTooltips.CompareMode)]
#if UNITY_EDITOR
        [OnValueChanged("@"+nameof(CheckModeType)+"("+nameof(Value)+","+nameof(Mode)+")")]
#endif
        [NinoMember(1)]
        public CompareMode Mode;
        [NinoMember(2)]
        public UInt64 Value;

        public override bool IsMatch(StoryPlayOverEvt obj, SceneGroup sceneGroup)
        {
            return IsMatch(Value, obj.StoryId, Mode);
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
