using Nino.Core;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [NinoType(false)]
    public abstract class ConfigAbilityAction
    {
        [NinoMember(1)][BoxGroup("Common")][LabelText("Targetting目标重新选定生效前过滤")]
        public ConfigAbilityPredicate Predicate;
        [NinoMember(2)][LabelText("Action目标")][BoxGroup("Common")]
        public AbilityTargetting Targetting;
        [NinoMember(3)][ShowIf(nameof(Targetting), AbilityTargetting.Other)][BoxGroup("Common")]
        public ConfigSelectTargets OtherTargets;
        [NinoMember(4)][BoxGroup("Common")][LabelText("Targetting目标重新选定生效后对每一个目标进行过滤")]
        public ConfigAbilityPredicate PredicateForeach; 
        protected abstract void Execute(Entity applier, ActorAbility ability, ActorModifier modifier, Entity target);

        public void DoExecute(Entity applier, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            if (Predicate == null || Predicate.Evaluate(applier,ability,modifier,target))
            {
                var res = AbilityHelper.ResolveTarget(applier, ability, modifier, target,Targetting, out Entity[] targetLs, OtherTargets);
                if (res > 0)
                {
                    foreach (Entity item in targetLs)
                    {
                        if (item != null)
                        {
                            if (PredicateForeach == null || PredicateForeach.Evaluate(applier, ability, modifier, item))
                            {
                                Execute(applier, ability, modifier, item);
                            }
                        }
                    }
                }
                else
                {
                    Log.Error("[ConfigAbilityAction:DoExecute] resolve action target is illegal,please check logic!");
                }
            }
        }
    }
}