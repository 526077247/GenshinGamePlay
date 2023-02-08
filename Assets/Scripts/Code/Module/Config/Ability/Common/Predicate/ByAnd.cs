namespace TaoTie
{
    /// <summary>
    /// 且
    /// </summary>
    public class ByAnd : ConfigAbilityPredicate
    {
        public ConfigAbilityPredicate[] Predicates;

        public override bool Evaluate(Entity actor, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            if (Predicates == null) return true;
            for (int i = 0; i < Predicates.Length; i++)
            {
                if (!Predicates[i].Evaluate(actor, ability, modifier, target))
                {
                    return false;
                }
            }

            return true;
        }
    }
}