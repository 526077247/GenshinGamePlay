namespace TaoTie
{
    public class ApplyModifier: ConfigAbilityAction
    {
        public string ModifierName;

        protected override void Execute(Entity applier, ActorAbility ability, ActorModifier modifier, Entity other)
        {
            var ac = other.GetComponent<AbilityComponent>();
            if (ac != null)
            {
                ac.ApplyModifier(applier.Id, ability, ModifierName);
            }
        }
    }
}