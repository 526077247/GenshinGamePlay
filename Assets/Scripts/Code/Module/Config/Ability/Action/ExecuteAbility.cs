namespace TaoTie
{
    public class ExecuteAbility: ConfigAbilityAction
    {
        public string AbilityName;

        protected override void Execute(Entity applier, ActorAbility ability, ActorModifier modifier, Entity aim)
        {
            var ac = aim.GetComponent<AbilityComponent>();
            if (ac != null)
            {
                ac.ExecuteAbility(AbilityName);
            }
        }
    }
}