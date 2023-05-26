using System;
using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [TriggerType(typeof(ConfigHeroNearPlatformEvtTrigger))]
    [NinoSerialize]
    public partial class ConfigHeroNearPlatformEvtIsMovingCondition : ConfigSceneGroupCondition<HeroNearPlatformEvt>
    {
        [Tooltip(SceneGroupTooltips.CompareMode)]
        [OnValueChanged("@"+nameof(CheckModeType)+"("+nameof(Value)+","+nameof(Mode)+")")]
        [NinoMember(1)]
        public CompareMode Mode;
        [NinoMember(2)]
        public Boolean Value;

        public override bool IsMatch(HeroNearPlatformEvt obj, SceneGroup sceneGroup)
        {
            return IsMatch(Value, obj.IsMoving, Mode);
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
