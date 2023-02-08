namespace TaoTie
{
    public class ExecuteAbility: ConfigAbilityAction
    {
        public string AbilityName;

        protected override void Execute(Entity applier, ActorAbility ability, ActorModifier modifier, Entity other)
        {
            var ac = other.GetComponent<AbilityComponent>();
            if (ac != null)
            {
                ac.ExecuteAbility(AbilityName);
            }
        }
    }
}