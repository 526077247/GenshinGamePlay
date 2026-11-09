using ProtoBuf;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 伤害吸收：把修饰器状态当作一个「伤害池」，受击时先扣池、扣完的溢出部分才落到 HP。
    ///
    /// <b>它只管规则，不管数据</b>。数据（池子有多大、怎么回、什么时候清零）
    /// 全部由 <see cref="ConfigAbilityModifier.ModifierSpecials"/> 声明，由
    /// <c>SetSpecial</c> / <c>AddSpecial</c> 修改，由
    /// <see cref="ConfigDoActionOnSpecialsChangeMixin"/> 监听。
    /// 所以这条 Mixin 的配置只有三项：扣哪个池、吃哪类伤害、排在谁前面。
    ///
    /// <b>典型用法（护盾）</b>：
    /// <code>
    /// ModifierSpecials: { MaxShield = 100, Shield = 100 }
    /// Mixins: [ ConfigAbsorbByState{ SpecialKey = Shield, Priority = 10 },
    ///           ConfigDoActionByTick{ AddSpecial(Shield, +10, ClampMax=MaxShield) },
    ///           ConfigDoActionOnSpecialsChange{ Key = Shield, Compare = LEqual, Value = 0,
    ///                                                Mode = FallingEdge, Actions = [...] } ]
    /// </code>
    /// 想按最大生命给盾（<c>MaxHpBase*0.5</c>），ModifierSpecials 是字面量表写不了公式，
    /// 改用一条 <c>ConfigDoActionAfterAddMixin</c>：
    /// <code>
    /// Actions: [ SetSpecial(MaxShield, Formula(MaxHpBase*0.5)),
    ///            SetSpecial(Shield, SpecialValue(MaxShield)) ]
    /// </code>
    /// ModifierSpecials 里仍建议留 <c>MaxShield = 0, Shield = 0</c> 占位，否则挂载时会警告「引用的状态没有初值」。
    /// 想加一面火盾，就再来一条 modifier，`SpecialKey` 还是 Shield、`Priority` 填更小的值、
    /// `MatchAttackTag` 填 Fire —— 两条互不干扰，火伤先吃火盾，溢出到通用盾。
    ///
    /// <b>为什么必须挂在 modifier 上</b>：状态是 per-target 的。挂在 ability 上时，
    /// 一个技能给三个目标加盾会共用同一份池子（ability 是施法者的），打第一个目标
    /// 三个人一起掉盾。这不是配置约束，是数据结构层面的错配，所以 ability 级会直接报错。
    ///
    /// <b>时点</b>：吸收发生在 <see cref="AttackHelper.DamageClose"/> 里、最终伤害算完之后。
    /// 不能提前到 BeforeBeAttack —— 那时 <see cref="AttackResult.DamagePercentage"/> 还只是
    /// 未经等级差 / 防御 / 暴击 / 子弹衰减的原料，池子无从判断「这一击要吃多少」。
    /// </summary>
    [ProtoContract]
    [LabelText("状态吸收伤害")]
    public partial class ConfigAbsorbByStateMixin : ConfigAbilityMixin
    {
        [ProtoMember(1)][LabelText("*状态名")]
        [Tooltip("从哪个状态扣。对应 ConfigAbilityModifier.ModifierSpecials 里的 key。\n" +
                 "留空或拼错会在挂载时报错 —— 它不像监听类「静默等不到」，而是直接不挡伤害。")]
        public string SpecialKey;

        [ProtoMember(2)][LabelText("只吸收这类攻击")]
        [Tooltip("填攻击标识（ConfigAttackInfo.AttackTag）的名字，留空 = 所有伤害都吃。\n" +
                 "例：火盾填 Fire，就只挡带 Fire 标识的攻击，其余伤害原样落到 HP。\n" +
                 "注意一个技能只能配一个 AttackTag；同时要「火 + 重击」两种分类时，" +
                 "这个字段表达不了，需要把 AttackTag 升级成位枚举。")]
        public string MatchAttackTag;

        [ProtoMember(3)][LabelText("吸收优先级")]
        [Tooltip("数值小的先扣。一次结算里命中多个池时按此排序，扣完一个再扣下一个。\n" +
                 "推荐：专盾（火盾/冰盾）填 0~9，通用盾填 10 以上 —— " +
                 "让专盾先吃它该吃的伤害，吃不下的溢出到通用盾。\n" +
                 "同优先级保持注册顺序（即 modifier 的挂载顺序），不可控，所以多盾时请显式区分。")]
        public int Priority;
    }
}
