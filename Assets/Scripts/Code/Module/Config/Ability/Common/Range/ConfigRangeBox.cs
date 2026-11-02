using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace TaoTie
{
    [ProtoContract]
    public partial class ConfigRangeBox: ConfigRange
    {
        [ProtoMember(10, IsRequired = true)] [NotNull] 
        public DynamicVector3 Size = new DynamicVector3();

        public override int ResolveEntity(Entity actor, ActorAbility ability, ActorModifier modifier, Entity target,
            EntityType[] filter, List<Entity> results)
        {
            var pos = bornType.ResolvePos(actor, ability, modifier, target);
            var rot = bornType.ResolveRot(actor, ability, modifier, target);
            var half = Size.Resolve(actor, ability) * 0.5f;
            if (filter == null) return results.Count;
            var em = actor?.Parent;
            if (em == null) return results.Count;
            var dict = em.GetAllDict();
            var inv = Quaternion.Inverse(rot); // 世界坐标 -> 盒子局部坐标
            foreach (var item in dict.Values)
            {
                if (item == null || item.IsDispose) continue;
                if (!(item is SceneEntity se)) continue; // 必须有坐标才能做判定
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
                var local = inv * (se.Position - pos);
                if (Mathf.Abs(local.x) <= half.x && Mathf.Abs(local.y) <= half.y && Mathf.Abs(local.z) <= half.z)
                {
                    results.Add(item);
                }
            }
            return results.Count;
        }
    }
}