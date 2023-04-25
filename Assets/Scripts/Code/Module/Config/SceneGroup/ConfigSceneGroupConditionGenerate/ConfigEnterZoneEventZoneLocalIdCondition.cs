using System;
using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [TriggerType(typeof(ConfigEnterZoneEventTrigger))]
    [NinoSerialize]
    public partial class ConfigEnterZoneEventZoneLocalIdCondition : ConfigSceneGroupCondition<EnterZoneEvent>
    {
        [Tooltip(SceneGroupTooltips.CompareMode)] [OnValueChanged("@CheckModeType(Value,Mode)")] 
        [NinoMember(1)]
        public CompareMode Mode;
        [NinoMember(2)]
        [ValueDropdown("@OdinDropdownHelper.GetSceneGroupZoneIds()")]
        public Int32 Value;

        public override bool IsMatch(EnterZoneEvent obj, SceneGroup sceneGroup)
        {
            return IsMatch(Value, obj.ZoneLocalId, Mode);
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
