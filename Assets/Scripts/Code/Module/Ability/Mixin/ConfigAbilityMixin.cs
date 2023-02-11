using Nino.Serialization;

namespace TaoTie
{
    [NinoSerialize]
    public abstract class ConfigAbilityMixin
    {
        public abstract AbilityMixin CreateAbilityMixin(ActorAbility actorAbility, ActorModifier actorModifier);
    }
}