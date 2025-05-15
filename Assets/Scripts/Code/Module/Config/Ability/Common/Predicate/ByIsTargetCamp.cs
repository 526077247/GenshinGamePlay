using Nino.Core;

namespace TaoTie
{
    [NinoType(false)]
    public partial class ByIsTargetCamp: ConfigAbilityPredicate
    {
        [NinoMember(10)]
        public AbilityTargetting CampBaseOn;
        [NinoMember(11)]
        public TargetType CampTargetType;
        public override bool Evaluate(Entity actor, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            using var entities = TargetHelper.ResolveTarget(actor, ability, modifier, target, Target);
            using var campBaseOnEntities = TargetHelper.ResolveTarget(actor, ability, modifier, target, CampBaseOn);
            if (entities.Count > 0 && campBaseOnEntities.Count > 0)
            {
                if(TargetHelper.IsTarget(entities[0] as Actor, campBaseOnEntities[0] as Actor, CampTargetType))
                {
                    return true;
                }
            }
            return false;
  
        }
    }
}