using Nino.Serialization;

namespace TaoTie
{
    /// <summary>
    /// 非
    /// </summary>
    [NinoSerialize]
    public class ByNot : ConfigAbilityPredicate
    {
        [NinoMember(10)]
        public ConfigAbilityPredicate Predicate;

        public override bool Evaluate(Entity actor, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            return !Predicate.Evaluate(actor, ability, modifier, target);
        }
    }
}