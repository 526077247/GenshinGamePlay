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
    [TriggerType(typeof(ConfigSuiteUnloadEventTrigger))]
    [ProtoContract]
    [LabelText("附加(true)还是替换(false)")]
    public partial class ConfigSuiteUnloadEventIsAddOnCondition : ConfigSceneGroupCondition<SuiteUnloadEvent>
    {
        [Tooltip(SceneGroupTooltips.CompareMode)]
#if UNITY_EDITOR
        [OnValueChanged("@"+nameof(CheckModeType)+"("+nameof(Value)+","+nameof(Mode)+")")]
#endif
        [ProtoMember(1)]
        [LabelText("判断类型")]
        public CompareMode Mode;
        [ProtoMember(2)]
        public Boolean Value;

        public override bool IsMatch(SuiteUnloadEvent obj, SceneGroup sceneGroup)
        {
            return IsMatch(Value, obj.IsAddOn, Mode);
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
