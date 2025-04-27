using Nino.Core;

namespace TaoTie
{
    [NinoType(false)]
    public partial class BySkillReady: ConfigAbilityPredicate
    {
        [NinoMember(10)]
        public int SkillId;

        public override bool Evaluate(Entity actor, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            using(var entities = TargetHelper.ResolveTarget(actor, ability, modifier, target, Target))
            {
                if (entities.Count > 0)
                {
                    var sc = entities[0].GetComponent<SkillComponent>();
                    if (sc != null)
                    {
                        return sc.IsSkillInCD(SkillId);
                    }
                    var ai = target.GetComponent<AIComponent>();
                    if (ai != null)
                    {
                        var skills = ai.GetSkillKnowledge().SkillsOnNerve.AvailableSkills;
                        for (int i = 0; i < skills.Count; i++)
                        {
                            if(skills[i].ConfigId == SkillId) return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}