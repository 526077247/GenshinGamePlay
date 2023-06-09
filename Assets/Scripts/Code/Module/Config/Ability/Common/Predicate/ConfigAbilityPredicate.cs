﻿using Nino.Serialization;

namespace TaoTie
{
    public abstract partial class ConfigAbilityPredicate
    {
        [NinoMember(1)]
        public AbilityTargetting Target;

        public abstract bool Evaluate(Entity actor, ActorAbility ability, ActorModifier modifier, Entity target);
    }
}