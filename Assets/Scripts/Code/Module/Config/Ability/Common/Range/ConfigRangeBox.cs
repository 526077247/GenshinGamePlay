using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    public class ConfigRangeBox: ConfigRange
    {
        public Vector3 Size;

        public override int ResolveEntity(Entity actor, ActorAbility ability, ActorModifier modifier, Entity target,
            EntityType[] filter, List<Entity> results)
        {
            var pos = bornType.ResolvePos(actor, ability, modifier, target);
            var rot = bornType.ResolveRot(actor, ability, modifier, target);
            int count = PhysicsHelper.OverlapBoxNonAlloc(pos, Size * 0.5f, rot, filter, out var res);
            for (int i = 0; i < count; i++)
            {
                var e = actor.Parent.Get<Entity>(res[i].EntityId);
                if (e != null)
                {
                    results.Add(e);
                }
            }
            return results.Count;
        }
    }
}