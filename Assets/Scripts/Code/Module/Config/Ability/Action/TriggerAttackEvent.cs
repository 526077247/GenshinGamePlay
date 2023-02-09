namespace TaoTie
{
    public class TriggerAttackEvent: ConfigAbilityAction
    {
        public TargetType TargetType;
        public ConfigAttackEvent AttackEvent;
        protected override void Execute(Entity applier, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            
        }
    }
}