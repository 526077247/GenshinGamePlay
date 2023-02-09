namespace TaoTie
{
    public class ByEntityTypes: ConfigAbilityPredicate
    {
        public EntityType[] EntityTypes;
        public override bool Evaluate(Entity actor, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            Entity[] targetLs = AbilityHelper.ResolveTarget(actor, ability, modifier, target, Target);
            if (targetLs != null && targetLs.Length > 0)
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