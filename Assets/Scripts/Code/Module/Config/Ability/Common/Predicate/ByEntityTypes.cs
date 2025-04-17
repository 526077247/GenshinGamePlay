using Nino.Core;

namespace TaoTie
{
    [NinoType(false)]
    public partial class ByEntityTypes: ConfigAbilityPredicate
    {
        [NinoMember(10)]
        public EntityType[] EntityTypes;
        public override bool Evaluate(Entity actor, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            using(var entities = TargetHelper.ResolveTarget(actor, ability, modifier, target, Target))
            {
                if (entities.Count > 0)
                {
                    for (int i = 0; i < EntityTypes.Length; i++)
                    {
                        if (entities[0].Type == EntityTypes[i])
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}