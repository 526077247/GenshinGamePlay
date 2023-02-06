namespace TaoTie
{
    public class BleedAction: ConfigAbilityAction
    {
        public int Num;

        protected override void Execute(Entity applier, ActorAbility ability, ActorModifier modifier, Entity other)
        {
            var nc = other.GetComponent<NumericComponent>();
            if (nc != null)
            {
                var now = nc.GetAsInt(NumericType.HpBase);
                now -= Num;
                nc.Set(NumericType.HpBase,now);
            }
        }
    }
}