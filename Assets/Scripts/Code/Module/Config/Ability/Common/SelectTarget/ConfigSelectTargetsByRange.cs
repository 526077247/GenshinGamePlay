
using System;

namespace TaoTie
{
    /// <summary>
    /// 通过范围筛选
    /// </summary>
    public class ConfigSelectTargetsByRange: ConfigSelectTargets
    {
        public ConfigRange Range;
        public EntityType[] EntityTypes;
        
        public override Entity[] ResolveTargets(Entity actor, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            using (ListComponent<Entity> list = ListComponent<Entity>.Create())
            {
                int result = Range.ResolveEntity(actor, ability, modifier, target,EntityTypes,list);
                if (result == 0) return Array.Empty<Entity>();
                return list.ToArray();
            }
        }
    }
}