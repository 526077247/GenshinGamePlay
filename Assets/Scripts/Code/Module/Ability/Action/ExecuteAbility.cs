using Nino.Serialization;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ExecuteAbility: ConfigAbilityAction
    {
        protected override void Execute(Entity applier, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            var ac = target.GetComponent<AbilityComponent>();
            if (ac != null)
            {
                ac.ExecuteAbility(ability.Config.AbilityName);
            }
        }
    }
}