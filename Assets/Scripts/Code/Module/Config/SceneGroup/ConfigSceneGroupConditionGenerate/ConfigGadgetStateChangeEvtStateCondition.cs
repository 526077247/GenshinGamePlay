using System;
using ProtoBuf;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif
using UnityEngine;

namespace TaoTie
{
    [TriggerType(typeof(ConfigGadgetStateChangeEvtTrigger))]
    [ProtoContract]
    [LabelText("新状态")]
    public partial class ConfigGadgetStateChangeEvtStateCondition : ConfigSceneGroupCondition<GadgetStateChangeEvt>
    {
        [Tooltip(SceneGroupTooltips.CompareMode)]
#if UNITY_EDITOR
        [OnValueChanged("@"+nameof(CheckModeType)+"("+nameof(Value)+","+nameof(Mode)+")")]
#endif
        [ProtoMember(1)]
        [LabelText("判断类型")]
        public CompareMode Mode;
        [ProtoMember(2)]
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
