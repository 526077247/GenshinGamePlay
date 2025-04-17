using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [NinoType(false)]
    public abstract class ConfigAbilityAction
    {
        [NinoMember(1)][BoxGroup("Common")][LabelText("*重定向前过滤")][Tooltip("Targetting目标重新选定生效前，判断当前Target是否满足条件执行")]
        public ConfigAbilityPredicate Predicate;
        [NinoMember(2)][LabelText("Action目标")][BoxGroup("Common")]
        public AbilityTargetting Targetting = AbilityTargetting.Target;
        [NinoMember(3)][ShowIf(nameof(Targetting), AbilityTargetting.Other)][BoxGroup("Common")]
        public ConfigSelectTargets OtherTargets;
        [NinoMember(4)][BoxGroup("Common")][LabelText("*重定向后过滤")][Tooltip("Targetting目标重新选定生效后，对每一个Target进行条件判断过滤")]
        public ConfigAbilityPredicate PredicateForeach; 
        protected abstract void Execute(Entity actionExecuter, ActorAbility ability, ActorModifier modifier, Entity target);

        public void DoExecute(Entity actionExecuter, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            if (Predicate == null || Predicate.Evaluate(actionExecuter, ability, modifier, target))
            {
                using (var entities =
                       TargetHelper.ResolveTarget(actionExecuter, ability, modifier, target, Targetting, OtherTargets))
                {
                    if (entities.Count == 0)
                    {
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
                                Execute(actionExecuter, ability, modifier, item);
                            }
                        }
                    }
                }
            }
        }
    }
}