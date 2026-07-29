using ProtoBuf;

namespace TaoTie
{
    /// <summary>
    /// 或
    /// </summary>
    [ProtoContract]
    public partial class ByOr : ConfigAbilityPredicate
    {
        [ProtoMember(10)]
        public ConfigAbilityPredicate[] Predicates;

        public override bool Evaluate(Entity actor, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            if (Predicates == null) return true;
            for (int i = 0; i < Predicates.Length; i++)
            {
                if (Predicates[i].Evaluate(actor, ability, modifier, target))
                {
                    return true;
                }
            }

            return false;
        }
    }
}