using System;
using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [TriggerType(typeof(ConfigGadgetStateChangeEvtTrigger))]
    [NinoType(false)]
    [LabelText("新状态")]
    public partial class ConfigGadgetStateChangeEvtStateCondition : ConfigSceneGroupCondition<GadgetStateChangeEvt>
    {
        [Tooltip(SceneGroupTooltips.CompareMode)]
#if UNITY_EDITOR
        [OnValueChanged("@"+nameof(CheckModeType)+"("+nameof(Value)+","+nameof(Mode)+")")]
#endif
        [NinoMember(1)]
        [LabelText("判断类型")]
        public CompareMode Mode;
        [NinoMember(2)]
        public GadgetState Value;

        public override bool IsMatch(GadgetStateChangeEvt obj, SceneGroup sceneGroup)
        {
            return IsMatch(Value, obj.State, Mode);
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
