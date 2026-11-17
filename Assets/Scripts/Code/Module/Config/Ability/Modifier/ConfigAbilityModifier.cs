using System.Collections.Generic;
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
        
        [ProtoMember(8, IsRequired = true)][LabelText("Modifier参数")]
        public Dictionary<string, float> ModifierSpecials;

        [ProtoMember(9)][LabelText("附着元素")]
        [Tooltip("这个 Modifier <b>挂到实体上时</b>给宿主挂什么元素，填元素表的 Id（ElementConfig）。0 = 不附着（默认）。\n" +
                 "这是<b>非伤害效果</b>的附着通道：给队友挂 buff、给自己上状态、场地效果 —— 它们没有伤害结算，\n" +
                 "但都有「一个 Modifier 挂到目标身上」这个共同点，所以在这里附着。\n" +
                 "<b>伤害型攻击不要用这个字段</b>，用 ConfigAttackProperty.Element / ElementDurability ——\n" +
                 "否则同一个元素会在 Modifier 挂上和命中结算时各挂一次。")]
        public int Element;

        [ProtoMember(10)][LabelText("附着元素量")][MinValue(0)]
        [Tooltip("挂上时入账多少元素量，整数（元素量不记小数），受该元素自身的耐久上限封顶。\n" +
                 "填 0 = 不挂（即使元素填了）。只在 Modifier 挂上那一刻生效一次，叠层不重复挂。")]
        public int ElementDurability;
    }
}
