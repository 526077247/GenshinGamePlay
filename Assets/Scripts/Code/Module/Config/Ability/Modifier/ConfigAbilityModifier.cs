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
        [ProtoMember(8)][LabelText("剥夺能力位")]
        [Tooltip("眩晕=CrowdControl(Move|Turn|CastSkill|Attack)、定身=Move、沉默=CastSkill。" +
                 "多个控制同时生效时取并集，不需要配互斥规则；留空(None)表示这不是控制类 buff")]
        public ControlFlag Deny;
        [ProtoMember(9)][LabelText("控制等级")]
        [Tooltip("仅用于表现层取强者（同时被眩晕和减速时播眩晕的动画/图标），不参与行为判定")]
        public int ControlLevel;
        [ProtoMember(10)][LabelText("生效时打断")]
        [Tooltip("从'未被控制'进入'被控制'那一刻执行一次；已处于控制中再叠加不会重复打断")]
        public ControlInterrupt Interrupt;
    }
}