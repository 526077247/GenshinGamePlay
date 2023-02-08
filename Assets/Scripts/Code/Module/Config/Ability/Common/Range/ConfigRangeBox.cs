using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    public class ConfigRangeBox: ConfigRange
    {
        public Vector3 Size;

        public override int ResolveEntity(Entity actor, ActorAbility ability, ActorModifier modifier, Entity target, EntityType[] filter,
            List<Entity> results)
        {
            throw new System.NotImplementedException();
        }
    }
}