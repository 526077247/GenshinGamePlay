namespace TaoTie
{
    /// <summary>
    /// 非
    /// </summary>
    public class ByNot : ConfigAbilityPredicate
    {
        public ConfigAbilityPredicate Predicate;

        public override bool Evaluate(Entity actor, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            return !Predicate.Evaluate(actor, ability, modifier, target);
        }
    }
}