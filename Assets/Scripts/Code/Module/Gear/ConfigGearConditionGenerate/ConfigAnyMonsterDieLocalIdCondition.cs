using System;
using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [LabelText("关卡内怪物死亡-怪物localId判断")]
    [TriggerType(typeof(ConfigAnyMonsterDieGearTrigger))]
    [NinoSerialize]
    public class ConfigAnyMonsterDieLocalIdCondition : ConfigGearCondition<AnyMonsterDieEvent>
    {
        [Tooltip(GearTooltips.CompareMode)] [OnValueChanged("@CheckModeType(value,mode)")]
        [NinoMember(1)]
        public CompareMode mode;
        [NinoMember(2)]
        [ValueDropdown("@OdinDropdownHelper.GetGearActorIds()")]
        public int value;
        
        public sealed override bool IsMatch(AnyMonsterDieEvent obj, Gear gear)
        {
            return IsMatch(obj.ActorId, value, mode);
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