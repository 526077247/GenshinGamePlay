using System.Collections.Generic;
using Nino.Core;
using UnityEngine;

namespace TaoTie
{
    [NinoType(false)]
    public partial class ConfigRangeBox: ConfigRange
    {
        [NinoMember(10)] [NotNull] 
        public DynamicVector3 Size = new DynamicVector3();

        public override int ResolveEntity(Entity actor, ActorAbility ability, ActorModifier modifier, Entity target,
            EntityType[] filter, List<Entity> results)
        {
            var pos = bornType.ResolvePos(actor, ability, modifier, target);
            var rot = bornType.ResolveRot(actor, ability, modifier, target);
            int count = PhysicsHelper.OverlapBoxNonAllocEntity(pos, Size.Resolve(actor,ability) * 0.5f, rot, filter, out var res);
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