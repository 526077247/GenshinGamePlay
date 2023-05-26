using System;
using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [TriggerType(typeof(ConfigVariableChangeEventTrigger))]
    [NinoSerialize]
    public partial class ConfigVariableChangeEventOldValueCondition : ConfigSceneGroupCondition<VariableChangeEvent>
    {
        [Tooltip(SceneGroupTooltips.CompareMode)]
        [OnValueChanged("@"+nameof(CheckModeType)+"("+nameof(Value)+","+nameof(Mode)+")")]
        [NinoMember(1)]
        public CompareMode Mode;
        [NinoMember(2)]
        public Single Value;

        public override bool IsMatch(VariableChangeEvent obj, SceneGroup sceneGroup)
        {
            return IsMatch(Value, obj.OldValue, Mode);
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
