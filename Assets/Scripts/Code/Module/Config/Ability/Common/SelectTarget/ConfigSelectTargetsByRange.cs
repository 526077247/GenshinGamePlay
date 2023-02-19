
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
        [NinoMember(3)]
        public TargetType CampTargetType;
        [NinoMember(4)]
        public AbilityTargetting CampBasedOn;
        public override Entity[] ResolveTargets(Entity actor, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            using (ListComponent<Entity> list = ListComponent<Entity>.Create())
            {
                int result = Range.ResolveEntity(actor, ability, modifier, target,EntityTypes,list);
                if (result == 0) return Array.Empty<Entity>();
                if (AbilityHelper.ResolveTarget(actor, ability, modifier, target, CampBasedOn,out var entities) > 0)
                {
                    if (entities[0] is Unit unit)
                    {
                        for (int i = list.Count-1; i >= 0 ; i--)
                        {
                            if (!(list[i] is Unit item) || !AbilityHelper.IsTarget(unit, item, CampTargetType))
                            {
                                list.RemoveAt(i);
                            }
                        }
                    }
                }
                return list.ToArray();
            }
        }
    }
}