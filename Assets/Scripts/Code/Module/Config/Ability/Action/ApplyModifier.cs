namespace TaoTie
{
    public class ApplyModifier: ConfigAbilityAction
    {
        public string ModifierName;

        public override void DoExecute(Entity applier, ActorAbility actorAbility, Entity other)
        {
            actorAbility.Parent.ApplyModifier(applier.Id, actorAbility, ModifierName);
        }
    }
}