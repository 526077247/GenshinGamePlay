﻿using Nino.Serialization;

namespace TaoTie
{
    public abstract class ConfigAbilityMixin
    {
        public abstract AbilityMixin CreateAbilityMixin(ActorAbility actorAbility, ActorModifier actorModifier);
    }
}