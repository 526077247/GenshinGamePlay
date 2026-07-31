using ProtoBuf;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif
using UnityEngine;

namespace TaoTie
{
    [ProtoContract]
    public partial class ConfigAbilityModifier
    {
        [ProtoMember(1)][Tooltip("当前Ability唯一")]
        public string ModifierName;
        [ProtoMember(2)][Tooltip("持续时间，-1无限，0瞬时，0+毫秒")]
        public int Duration;
        [ProtoMember(3)][ShowIf("@"+nameof(Duration)+"!=0")]
        public StackingType StackingType;
        [ProtoMember(4)][ShowIf(nameof(StackingType),StackingType.Multiple)]
        public int StackLimitCount;
        [ProtoMember(5)][LabelText("*Mixins")][Tooltip("其中所有Action的默认applier都为ability持有者")]
        public ConfigAbilityMixin[] Mixins;
        [ProtoMember(6)][LabelText("修改玩家数值")]
        public ConfigCombatProperty[] Properties;
    }
}