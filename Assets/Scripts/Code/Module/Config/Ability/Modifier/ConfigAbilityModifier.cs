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
        
        [ProtoMember(7)][LabelText("状态标记(Tags)")][EnumToggleButtons]
        [Tooltip("实体'处于什么状态 / 因为什么受限'。代码查询只认标记、不认 ModifierName —— " +
                 "名字是实例身份，标记是语义。\n" +
                 "• 段1 控制身份（Stun/Freeze/Root/Silence/Slow/Petrify/Charm/Fear）：只能选 1 个，" +
                 "剥夺哪些能力、要不要打断当前动作、表现等级多高，全部由 GameplayTagMeta 自动派生 —— " +
                 "眩晕就是'能力被剥夺'这件事的名字，不需要也不能手写剥夺（剥夺位不在这个枚举里）。\n" +
                 "• 段2 动作锁（Chant/Charge/Stagger/Recover）：只能选 1 个，机制同段1，但不算被控。\n" +
                 "• 段3 状态标记（Burn/Poison/Bleed/Vulnerable/Shield/Rush）：可多选，不剥夺能力，" +
                 "层数上限挂在标记上（GameplayTagMeta.maxStackOf），同一标记的所有来源共享一个池。")]
        public GameplayTags Tags;
    }
}