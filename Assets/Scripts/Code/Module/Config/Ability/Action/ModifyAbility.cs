namespace TaoTie
{
    public class ModifyAbility: ConfigAbilityAction
    {
        public string Key;
        public float Value;
        protected override void Execute(Entity applier, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            ability.SetSpecials(Key, Value);
        }
    }
}