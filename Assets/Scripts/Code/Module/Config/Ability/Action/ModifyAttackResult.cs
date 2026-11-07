using ProtoBuf;
using UnityEngine;
#if UNITY_EDITOR
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif
#endif

namespace TaoTie
{
    /// <summary>
    /// 修改**本次这一击**的结算参数（伤害、倍率、暴击、额外伤害，或直接取消）。
    ///
    /// 只能放在攻击类钩子里用：<see cref="ConfigDoActionBeforeAttackMixin"/>（攻击方，Target 是受击者）
    /// 或 <see cref="ConfigDoActionBeforeBeAttackMixin"/>（受击方，Target 是攻击者）。
    /// 两个钩子都在 <see cref="AttackHelper.DamageClose"/> 里、防御/暴击公式执行**之前**，
    /// 所以这里改的值会完整参与结算。
    ///
    /// 三条必须知道的边界：
    /// 1. <b>只对当前这一击有效</b>：<see cref="AttackResult"/> 由钩子经
    ///    <see cref="ConfigAbilityAction.DoExecute"/> 的参数传进来，钩子返回即失效。
    ///    想在 <see cref="DelayDoAction"/> 里延时执行是无效的（那时拿不到 result，会报错）。
    /// 2. <b>数值解析上下文是钩子的 Target</b>：受击前钩子里 Target 是攻击者，所以
    ///    <see cref="NumericValue"/> 读到的是攻击方的属性（"增伤随攻方 ATK 成长"就靠这个）；
    ///    而 <see cref="FormulaValue"/> 里的 ability specials 取自**挂载这条 mixin 的 ability**，
    ///    不是攻击方的 —— 想做"按攻方技能等级增伤"要显式读属性，不要指望 specials。
    ///
    /// 加法顺序无所谓：<c>DamagePercentageRatio</c> 加在防御减免前还是后，
    /// 相对增幅都是同一个比例（乘法交换律）。只有加在 <see cref="DamageExtraAdd"/> 上
    /// 才是"不被防御和倍率影响的固定伤害"。
    /// </summary>
    [ProtoContract]
    public partial class ModifyAttackResult : ConfigAbilityAction
    {
#if UNITY_EDITOR
        [LabelText("击打类型条件 None表示任意")]
#endif
        [ProtoMember(10)]
        public StrikeType StrikeType;
#if UNITY_EDITOR
        [LabelText("攻击类型条件 None表示任意")]
#endif
        [ProtoMember(17)]
        public AttackType AttackType;
        [ProtoMember(11)]
#if UNITY_EDITOR
        [LabelText("受伤倍率加成")]
        [Tooltip("加到 DamagePercentageRatio 上：0.3 = 这一击 +30%。\n" +
                 "同一次结算里多个来源**相加**（0.3 + 0.2 = +50%），想做叠乘就自己算好填。\n" +
                 "破韧期增伤就填这个：挂在破韧时加到目标身上的 modifier 里，窗口 == 眩晕窗口")]
#endif
        public BaseValue DamageRatioAdd;

        [ProtoMember(12)]
#if UNITY_EDITOR
        [LabelText("基础伤害加成")]
        [Tooltip("加到 DamagePercentage 上（攻击方的技能基础伤害），会被防御、倍率、暴击一起放大")]
#endif
        public BaseValue DamageAdd;

        [ProtoMember(13)]
#if UNITY_EDITOR
        [LabelText("额外伤害加成")]
        [Tooltip("加到 DamageExtra 上：固定伤害，不吃防御、不吃倍率、不吃暴击（与原 DamageExtra 同语义）")]
#endif
        public BaseValue DamageExtraAdd;

        [ProtoMember(14)]
#if UNITY_EDITOR
        [LabelText("暴击率加成")]
        [Tooltip("加到 BonusCritical 上，1 = +100% 必暴")]
#endif
        public BaseValue CriticalRateAdd;

        [ProtoMember(15)]
#if UNITY_EDITOR
        [LabelText("暴伤加成")]
        [Tooltip("加到 BonusCriticalHurt 上：0.5 = 暴击时再多 50%")]
#endif
        public BaseValue CriticalHurtAdd;

        [ProtoMember(16)]
#if UNITY_EDITOR
        [LabelText("取消这一击")]
        [Tooltip("复用现有的 IsEffective 语义：置 false 后 DamageClose 直接返回，不扣血、不削韧、不出特效。\n" +
                 "闪避/无敌/免疫/格挡都可以用它表达，不需要再开一套机制")]
#endif
        public bool Cancel;

        /// <summary>
        /// 只会被"没带 result 的调用链"命中（Tick / AfterAdd / OnUseSkill 等），属于配置错误：
        /// 转给带 result 的重载，让它在 result 为 null 时报一条明确的错误。
        /// </summary>
        protected override void Execute(Entity actionExecuter, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            Execute(actionExecuter, ability, modifier, target, null);
        }

        protected override void Execute(Entity actionExecuter, ActorAbility ability, ActorModifier modifier, Entity target, AttackResult result)
        {
            if (result == null)
            {
                // 挂在了非攻击钩子（Tick/AfterAdd 等）上，或者在 DelayDoAction 里延时调用了。
                // 这是配置错误，必须吵出来 —— 静默 return 会让人以为"配了但没生效"是数值问题。
                Log.Error($"ModifyAttackResult 只能用于攻击钩子（BeforeAttack/BeforeBeAttack），" +
                          $"当前上下文里没有正在结算的攻击：{ability?.Config?.AbilityName}");
                return;
            }

            if (StrikeType != StrikeType.None && StrikeType != result.StrikeType) return;
            if (AttackType != AttackType.None && AttackType != result.AttackType) return;
            // 解析上下文用 target（钩子传入的对手），拿不到才退回 executer。
            // 受击前钩子里 target 就是攻击者，这正是"按攻方属性缩放增伤"想要的上下文。
            var ctx = target ?? actionExecuter;

            if (DamageRatioAdd != null)
                result.DamagePercentageRatio += DamageRatioAdd.Resolve(ctx, ability, modifier);
            if (DamageAdd != null)
                result.DamagePercentage += DamageAdd.Resolve(ctx, ability, modifier);
            if (DamageExtraAdd != null)
                result.DamageExtra += DamageExtraAdd.Resolve(ctx, ability, modifier);
            if (CriticalRateAdd != null)
                result.BonusCritical += CriticalRateAdd.Resolve(ctx, ability, modifier);
            if (CriticalHurtAdd != null)
                result.BonusCriticalHurt += CriticalHurtAdd.Resolve(ctx, ability, modifier);

            if (Cancel)
                result.IsEffective = false;
        }
    }
}
