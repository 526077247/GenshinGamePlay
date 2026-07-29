using ProtoBuf;

namespace TaoTie
{
    /// <summary>
    /// 非
    /// </summary>
    [ProtoContract]
    public partial class ByNot : ConfigAbilityPredicate
    {
        [ProtoMember(10)]
        public ConfigAbilityPredicate Predicate;

        public override bool Evaluate(Entity actor, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            return !Predicate.Evaluate(actor, ability, modifier, target);
        }
    }
}