using System;
using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [TriggerType(typeof(ConfigGadgetStateChangeEvtTrigger))]
    [NinoSerialize]
    public partial class ConfigGadgetStateChangeEvtGadgetIdCondition : ConfigSceneGroupCondition<GadgetStateChangeEvt>
    {
        [Tooltip(SceneGroupTooltips.CompareMode)]
        [OnValueChanged("@"+nameof(CheckModeType)+"("+nameof(Value)+","+nameof(Mode)+")")]
        [NinoMember(1)]
        public CompareMode Mode;
        [NinoMember(2)]
        [ValueDropdown("@"+nameof(OdinDropdownHelper)+"."+nameof(OdinDropdownHelper.GetSceneGroupActorIds)+"()",AppendNextDrawer = true)]
        public Int32 Value;

        public override bool IsMatch(GadgetStateChangeEvt obj, SceneGroup sceneGroup)
        {
            return IsMatch(Value, obj.GadgetId, Mode);
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
