using ProtoBuf;

namespace TaoTie
{
    [ProtoContract]
    [ProtoInclude(100, typeof(ByAnd))]
    [ProtoInclude(101, typeof(ByEntityTypes))]
    [ProtoInclude(102, typeof(ByIsTargetCamp))]
    [ProtoInclude(103, typeof(ByNot))]
    [ProtoInclude(104, typeof(ByOr))]
    [ProtoInclude(105, typeof(BySkillReady))]
    public abstract partial class ConfigAbilityPredicate
    {
        [ProtoMember(1)]
        public AbilityTargetting Target;

        public abstract bool Evaluate(Entity actor, ActorAbility ability, ActorModifier modifier, Entity target);
    }
}