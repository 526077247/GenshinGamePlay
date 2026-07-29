
using System;
using ProtoBuf;

namespace TaoTie
{
    /// <summary>
    /// 通过范围筛选
    /// </summary>
    [ProtoContract]
    public partial class ConfigSelectTargetsByRange: ConfigSelectTargets
    {
        [ProtoMember(1)][NotNull]
        public ConfigRange Range;
        [ProtoMember(2)]
        public EntityType[] EntityTypes;
        [ProtoMember(3)]
        public TargetType CampTargetType;
        [ProtoMember(4)]
        public AbilityTargetting CampBasedOn;
        public override ListComponent<Entity> ResolveTargets(Entity actor, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            ListComponent<Entity> list = ListComponent<Entity>.Create();
            int result = Range.ResolveEntity(actor, ability, modifier, target, EntityTypes, list);
            if (result == 0) return list;
            using(var entities = TargetHelper.ResolveTarget(actor, ability, modifier, target, CampBasedOn))
            {
                if (entities.Count>0 && entities[0] is Actor unit)
                {
                    for (int i = list.Count-1; i >= 0 ; i--)
                    {
                        if (!(list[i] is Actor item) || !TargetHelper.IsTarget(unit, item, CampTargetType))
                        {
                            list.RemoveAt(i);
                        }
                    }
                }
            }
            return list;
        }
    }
}