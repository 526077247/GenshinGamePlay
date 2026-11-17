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
    public partial class ConfigAttackProperty
    {
        [ProtoMember(1)][LabelText("*伤害值")] [NotNull]
        [Tooltip("受暴击、伤害比例、等级差距衰减等所有因素影响,最终结果最低为0不会为负数,即不可用于实现生命回复功能")]
        public BaseValue DamagePercentage = new SingleValue();
        [ProtoMember(2, IsRequired = true)][LabelText("*伤害比例(小数)")] [NotNull][Tooltip("小数,1为100%")]
        public BaseValue DamagePercentageRatio = new SingleValue(1);
        [ProtoMember(3)][LabelText("击打类型")]
        public StrikeType StrikeType;
        [ProtoMember(4)][LabelText("破霸体")]
        [Tooltip("逐条配置：某部位对某属性削多少。留空则该攻击不削韧")]
        public List<ConfigNumericBreak> NumericBreak = new List<ConfigNumericBreak>();
        [ProtoMember(5)][LabelText("攻击类型")]
        public AttackType AttackType;
        [ProtoMember(6, IsRequired = true)][LabelText("*额外伤害值")][NotNull][Tooltip("暂定为不受任何因素影响的固定伤害,如默认可以给1,防止伤害最终结算为0")]
        public BaseValue DamageExtra = new SingleValue(1);
        [ProtoMember(7, IsRequired = true)][LabelText("*暴击率(小数)")][NotNull][Tooltip("小数,1为100%")]
        public BaseValue BonusCritical = new ZeroValue();
        [ProtoMember(8)][LabelText("*额外暴击伤害(小数)")][NotNull]
        [Tooltip("小数,不考虑其他因素时默认暴击伤害系数为1+该设定值. 例：未暴击伤害为100,当该值为0时暴击伤害为100x(1+0)=100,当该值为0.5时暴击伤害为100x(1+0.5)=150")]
        public BaseValue BonusCriticalHurt = new SingleValue(1);
        [ProtoMember(9)][LabelText("*忽略等级差距带来的衰减")]
        [Tooltip("默认等级差系数范围(0,2)公式为: Fx双曲正切值((攻击方等级-防御方等级)/10) + 1")]
        public bool IgnoreLevelDiff;
        [ProtoMember(10)][LabelText("*是否真伤")] [Tooltip("真伤可以忽视对方防御影响,默认防御减伤比例系数范围(0,1)公式为：(攻击方等级*100)/(防守方防御值+攻击方等级*100)")]
        public bool TrueDamage;

        [ProtoMember(11)][LabelText("元素")]
        [Tooltip("这一击「是什么元素」，填元素表的 Id（ElementConfig）。0 = 不带元素。\n" +
                 "决定命中后附着什么元素、能触发哪些反应；<b>不影响伤害</b> —— 元素增伤请在配置层用 Action 自己表达。")]
        public int Element;

        [ProtoMember(12)][LabelText("元素量")]
        [Tooltip("命中后附着多少元素量（整数，向下取整）。0 = 有元素类型但不附着。\n" +
                 "只决定「挂多少」，不影响伤害。\n" +
                 "<b>非伤害效果</b>（给自己或队友挂 buff 之类）不走这里，用 ConfigAbilityModifier 的「附着元素」——\n" +
                 "那条在 Modifier 挂到实体上时生效，不需要有伤害结算。两条都配会让同一个元素挂两次。")]
        public int ElementDurability;

        // 原 ProtoMember(13)「消耗已附着元素」、14「消耗量」已删除：
        // 命中时抽走对方元素改由配置层的 ConsumeElement Action 负责，攻击属性不再自带消耗字段。
        // 号段空出不复用，避免已有配置资产里的旧值被新字段误读。
    }
}
