using System;
using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [TriggerType(typeof(ConfigAnyMonsterDieEventTrigger))]
    [NinoSerialize]
    public partial class ConfigAnyMonsterDieEventActorIdCondition : ConfigSceneGroupCondition<AnyMonsterDieEvent>
    {
        [Tooltip(SceneGroupTooltips.CompareMode)] [OnValueChanged("@CheckModeType(value,mode)")] 
        [NinoMember(1)]
        public CompareMode mode;
        [NinoMember(2)]
        [ValueDropdown("@OdinDropdownHelper.GetSceneGroupActorIds()")]
        public Int32 value;

        public override bool IsMatch(AnyMonsterDieEvent obj,SceneGroup sceneGroup)
        {
            return IsMatch(value, obj.ActorId, mode);
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
