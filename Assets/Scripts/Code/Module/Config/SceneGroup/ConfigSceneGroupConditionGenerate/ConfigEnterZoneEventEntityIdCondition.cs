using System;
using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [TriggerType(typeof(ConfigEnterZoneEventTrigger))]
    [NinoType(false)]
    public partial class ConfigEnterZoneEventEntityIdCondition : ConfigSceneGroupCondition<EnterZoneEvent>
    {
        [Tooltip(SceneGroupTooltips.CompareMode)]
#if UNITY_EDITOR
        [OnValueChanged("@"+nameof(CheckModeType)+"("+nameof(Value)+","+nameof(Mode)+")")]
#endif
        [NinoMember(1)]
        public CompareMode Mode;
        [NinoMember(2)]
        public Int64 Value;

        public override bool IsMatch(EnterZoneEvent obj, SceneGroup sceneGroup)
        {
            return IsMatch(Value, obj.EntityId, Mode);
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
