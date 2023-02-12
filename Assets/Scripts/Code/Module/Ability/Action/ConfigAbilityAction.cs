using Nino.Serialization;
using Sirenix.OdinInspector;

namespace TaoTie
{
    public abstract class ConfigAbilityAction
    {
        [NinoMember(1)][LabelText("Action目标")][BoxGroup("Common")]
        public AbilityTargetting Targetting;
        [NinoMember(2)][ShowIf("@Targetting == AbilityTargetting.Other")][BoxGroup("Common")]
        public ConfigSelectTargets OtherTargets;
        [NinoMember(3)][BoxGroup("Common")]
        public ConfigAbilityPredicate Predicate;
        [NinoMember(4)][BoxGroup("Common")]
        public ConfigAbilityPredicate PredicateForeach; 
        protected abstract void Execute(Entity applier, ActorAbility ability, ActorModifier modifier, Entity target);

        public void DoExecute(Entity applier, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            if (Predicate == null || Predicate.Evaluate(applier,ability,modifier,target))
            {
                Entity[] targetLs = AbilityHelper.ResolveTarget(applier, ability, modifier, target,Targetting, OtherTargets);
                if (targetLs != null && targetLs.Length > 0)
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