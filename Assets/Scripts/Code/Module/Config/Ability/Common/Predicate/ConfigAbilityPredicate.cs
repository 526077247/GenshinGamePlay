using Nino.Serialization;

namespace TaoTie
{
    [NinoSerialize]
    public abstract class ConfigAbilityPredicate
    {
        [NinoMember(1)]
        public AbilityTargetting Target;

        public abstract bool Evaluate(Entity actor, ActorAbility ability, ActorModifier modifier, Entity target);
    }
}