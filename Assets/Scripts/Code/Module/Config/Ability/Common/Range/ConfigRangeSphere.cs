using System.Collections.Generic;

namespace TaoTie
{
    public class ConfigRangeSphere: ConfigRange
    {
        public float Radius;

        public override int ResolveEntity(Entity actor, ActorAbility ability, ActorModifier modifier, Entity target, EntityType[] filter,
            List<Entity> results)
        {
            throw new System.NotImplementedException();
        }
    }
}