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
    [ProtoInclude(100, typeof(AddAbility))]
    [ProtoInclude(101, typeof(AddNumericValue))]
    [ProtoInclude(102, typeof(AddSkillInfo))]
    [ProtoInclude(103, typeof(ApplyModifier))]
    [ProtoInclude(104, typeof(AttachEffect))]
    [ProtoInclude(105, typeof(AttachModifier))]
    [ProtoInclude(106, typeof(CreateGadget))]
    [ProtoInclude(107, typeof(DelayDoAction))]
    [ProtoInclude(108, typeof(EnableHitBoxByName))]
    [ProtoInclude(109, typeof(EnableRenderer))]
    [ProtoInclude(110, typeof(ExecuteAbility))]
    [ProtoInclude(111, typeof(KillGadget))]
    [ProtoInclude(112, typeof(KillSelf))]
    [ProtoInclude(113, typeof(ModifyAbility))]
    [ProtoInclude(114, typeof(OpenView))]
    [ProtoInclude(115, typeof(Predicated))]
    [ProtoInclude(116, typeof(PrintTextAction))]
    [ProtoInclude(117, typeof(RemoveAbility))]
    [ProtoInclude(118, typeof(RemoveModifier))]
    [ProtoInclude(119, typeof(SetGadgetState))]
    [ProtoInclude(120, typeof(ShowIntee))]
    [ProtoInclude(121, typeof(TargetAttackEvent))]
    [ProtoInclude(122, typeof(TriggerAttackEvent))]
    [ProtoInclude(123, typeof(TriggerSkillCD))]
    [ProtoInclude(124, typeof(TryDoSkill))]
    [ProtoInclude(125, typeof(SetFsmParam<bool>))]
    [ProtoInclude(126, typeof(SetFsmParam<float>))]
    [ProtoInclude(127, typeof(SetFsmParam<int>))]
    [ProtoInclude(128, typeof(SampleHoldInputAction))]
    [ProtoInclude(129, typeof(RemoveEffect))]
    [ProtoInclude(130, typeof(ShowRangeAction))]
    [ProtoInclude(131, typeof(SetNumericValue))]
    [ProtoInclude(132, typeof(ModifyAttackResult))]
    [ProtoInclude(133, typeof(RemoveModifierByTag))]
    public abstract class ConfigAbilityAction
    {
        [ProtoMember(1)][BoxGroup("Common")][LabelText("*重定向前过滤")][Tooltip("Targetting目标重新选定生效前，判断当前Target是否满足条件执行")]
        public ConfigAbilityPredicate Predicate;
        [ProtoMember(2, IsRequired = true)][LabelText("Action目标")][BoxGroup("Common")]
        public AbilityTargetting Targetting = AbilityTargetting.Target;
        [ProtoMember(3)][ShowIf(nameof(Targetting), AbilityTargetting.Other)][BoxGroup("Common")]
        public ConfigSelectTargets OtherTargets;
        [ProtoMember(4)][BoxGroup("Common")][LabelText("*重定向后过滤")][Tooltip("Targetting目标重新选定生效后，对每一个Target进行条件判断过滤")]
        public ConfigAbilityPredicate PredicateForeach; 
        protected abstract void Execute(Entity actionExecuter, ActorAbility ability, ActorModifier modifier, Entity target);

        /// <summary>
        /// 需要"本次结算"信息的 Action 覆写这个重载（比如 <see cref="ModifyAttackResult"/>）。
        ///
        /// 默认直接转发到 4 参版本，所以其余 30 多个 Action 一行都不用改。
        /// 只有攻击类钩子（BeforeAttack / BeforeBeAttack / AfterAttack / AfterBeAttack）会传非 null 的
        /// <paramref name="result"/>；其余链路（Tick、AfterAdd、OnUseSkill…）没有"本次这一击"，
        /// 传进来的是 null —— 覆写方要自己判空并报错，别静默吞掉。
        /// </summary>
        protected virtual void Execute(Entity actionExecuter, ActorAbility ability, ActorModifier modifier,
            Entity target, AttackResult result)
        {
            Execute(actionExecuter, ability, modifier, target);
        }

        /// <param name="result">本次结算的伤害句柄；非攻击钩子为 null</param>
        public void DoExecute(Entity actionExecuter, ActorAbility ability, ActorModifier modifier, Entity target, AttackResult result = null)
        {
            if (Predicate == null || Predicate.Evaluate(actionExecuter, ability, modifier, target))
            {
                using (var entities =
                       TargetHelper.ResolveTarget(actionExecuter, ability, modifier, target, Targetting, OtherTargets))
                {
                    if (entities.Count == 0)
                    {
                        if(Targetting == AbilityTargetting.Other)
                            Log.Info("没有找到Other范围内的Target");
                        else
                            Log.Error("没有找到重定向Target，请检查逻辑");
                        return;
                    }

                    foreach (Entity item in entities)
                    {
                        if (item != null)
                        {
                            if (PredicateForeach == null ||
                                PredicateForeach.Evaluate(actionExecuter, ability, modifier, item))
                            {
                                Execute(actionExecuter, ability, modifier, item, result);
                            }
                        }
                    }
                }
            }
        }
    }
}