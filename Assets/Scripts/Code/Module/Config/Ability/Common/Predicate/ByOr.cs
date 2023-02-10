using Nino.Serialization;

namespace TaoTie
{
    /// <summary>
    /// 或
    /// </summary>
    [NinoSerialize]
    public partial class ByOr : ConfigAbilityPredicate
    {
        [NinoMember(10)]
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