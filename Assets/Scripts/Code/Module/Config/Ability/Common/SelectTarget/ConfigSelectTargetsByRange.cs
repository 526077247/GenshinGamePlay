
using System;
using Nino.Serialization;

namespace TaoTie
{
    /// <summary>
    /// 通过范围筛选
    /// </summary>
    [NinoSerialize]
    public partial class ConfigSelectTargetsByRange: ConfigSelectTargets
    {
        [NinoMember(1)][NotNull]
        public ConfigRange Range;
        [NinoMember(2)]
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