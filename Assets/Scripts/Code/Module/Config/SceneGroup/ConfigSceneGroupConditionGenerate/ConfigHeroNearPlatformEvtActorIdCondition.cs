using System;
using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [TriggerType(typeof(ConfigHeroNearPlatformEvtTrigger))]
    [NinoSerialize]
    public partial class ConfigHeroNearPlatformEvtActorIdCondition : ConfigSceneGroupCondition<HeroNearPlatformEvt>
    {
        [Tooltip(SceneGroupTooltips.CompareMode)] [OnValueChanged("@CheckModeType(value,mode)")] 
        [NinoMember(1)]
        public CompareMode mode;
        [NinoMember(2)]
        [ValueDropdown("@OdinDropdownHelper.GetSceneGroupActorIds()")]
        public Int32 value;

        public override bool IsMatch(HeroNearPlatformEvt obj, SceneGroup sceneGroup)
        {
            return IsMatch(value, obj.actorId, mode);
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
