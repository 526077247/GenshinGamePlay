using Nino.Core;

namespace TaoTie
{
    [NinoType(false)]
    public abstract partial class ConfigAbilityMixin
    {
        public abstract AbilityMixin CreateAbilityMixin(ActorAbility actorAbility, ActorModifier actorModifier);
    }
}