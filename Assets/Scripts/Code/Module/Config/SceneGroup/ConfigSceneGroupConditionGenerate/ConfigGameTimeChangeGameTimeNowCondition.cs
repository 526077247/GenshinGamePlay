using System;
using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [TriggerType(typeof(ConfigGameTimeChangeTrigger))]
    [NinoType(false)]
    [LabelText("游戏时间(ms)")]
    public partial class ConfigGameTimeChangeGameTimeNowCondition : ConfigSceneGroupCondition<GameTimeChange>
    {
        [Tooltip(SceneGroupTooltips.CompareMode)]
#if UNITY_EDITOR
        [OnValueChanged("@"+nameof(CheckModeType)+"("+nameof(Value)+","+nameof(Mode)+")")]
#endif
        [NinoMember(1)]
        [LabelText("判断类型")]
        public CompareMode Mode;
        [NinoMember(2)]
        public Int64 Value;

        public override bool IsMatch(GameTimeChange obj, SceneGroup sceneGroup)
        {
            return IsMatch(Value, obj.GameTimeNow, Mode);
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
