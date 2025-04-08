using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [NinoType(false)]
    public partial class ConfigAbilityModifier
    {
        [NinoMember(1)][Tooltip("当前Ability唯一")]
        public string ModifierName;
        [NinoMember(2)][Tooltip("持续时间，-1无限，0瞬时，0+毫秒")]
        public int Duration;
        [NinoMember(3)][ShowIf("@"+nameof(Duration)+"!=0")]
        public StackingType StackingType;
        [NinoMember(4)][ShowIf(nameof(StackingType),StackingType.Multiple)]
        public int StackLimitCount;
        [NinoMember(5)][LabelText("*Mixins")][Tooltip("其中所有Action的默认applier都为ability持有者")]
        public ConfigAbilityMixin[] Mixins;
        [NinoMember(6)][LabelText("修改玩家数值")]
        public ConfigCombatProperty[] Properties;
    }
}