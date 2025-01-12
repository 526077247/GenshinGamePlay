using System.Collections.Generic;
using Nino.Core;

namespace TaoTie
{
    [NinoType(false)]
    public partial class ConfigRangeSphere: ConfigRange
    {
        [NinoMember(10)][NotNull]
        public BaseValue Radius;

        public override int ResolveEntity(Entity actor, ActorAbility ability, ActorModifier modifier, Entity target, EntityType[] filter,
            List<Entity> results)
        {
            var pos = bornType.ResolvePos(actor, ability, modifier, target);
            int count = PhysicsHelper.OverlapSphereNonAllocEntity(pos, Radius.Resolve(actor,ability), filter, out var res);
            for (int i = 0; i < count; i++)
            {
                var e = actor.Parent.Get<Entity>(res[i]);
                if (e != null)
                {
                    results.Add(e);
                }
            }
            return results.Count;
        }
    }
}