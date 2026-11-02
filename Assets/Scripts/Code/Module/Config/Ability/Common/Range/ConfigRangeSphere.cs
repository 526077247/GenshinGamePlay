using System.Collections.Generic;
using ProtoBuf;

namespace TaoTie
{
    [ProtoContract]
    public partial class ConfigRangeSphere: ConfigRange
    {
        [ProtoMember(10)][NotNull]
        public BaseValue Radius;

        public override int ResolveEntity(Entity actor, ActorAbility ability, ActorModifier modifier, Entity target, EntityType[] filter,
            List<Entity> results)
        {
            var pos = bornType.ResolvePos(actor, ability, modifier, target);
            var radiusSqr = Radius.Resolve(actor, ability);
            radiusSqr *= radiusSqr;
            if (filter == null) return results.Count;
            var em = actor?.Parent;
            if (em == null) return results.Count;
            var dict = em.GetAllDict();
            foreach (var item in dict.Values)
            {
                if (item == null || item.IsDispose) continue;
                if (!(item is SceneEntity se)) continue; // 必须有坐标才能做距离判定
                var type = item.Type;
                bool match = false;
                for (int j = 0; j < filter.Length; j++)
                {
                    if (type == filter[j])
                    {
                        match = true;
                        break;
                    }
                }
                if (!match) continue;
                var offset = se.Position - pos;
                if (offset.sqrMagnitude <= radiusSqr)
                {
                    results.Add(item);
                }
            }
            return results.Count;
        }
    }
}