namespace TaoTie
{
    public abstract class ConfigAbilityPredicate
    {
        public AbilityTargetting Target;

        public abstract bool Evaluate(Entity actor, ActorAbility ability, ActorModifier modifier, Entity target);
    }
}