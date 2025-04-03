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
            var res = AbilitySystem.ResolveTarget(actor, ability, modifier, target, Target,out Entity[] targetLs);
            if (res > 0)
            {
                for (int i = 0; i < EntityTypes.Length; i++)
                {
                    if (targetLs[0].Type == EntityTypes[i])
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}