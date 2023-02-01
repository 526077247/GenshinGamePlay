namespace TaoTie
{
    public class ApplyModifier: ConfigAbilityAction
    {
        public string ModifierName;

        public override void DoExecute(ActorAbility actorAbility, Entity other)
        {
            actorAbility.Parent.ApplyModifier(actorAbility.Parent.Id,actorAbility,ModifierName);
        }
    }
}