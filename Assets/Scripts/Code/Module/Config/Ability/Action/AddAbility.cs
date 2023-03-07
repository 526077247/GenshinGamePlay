using Nino.Serialization;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [NinoSerialize]
    public partial class AddAbility: ConfigAbilityAction
    {
        [NinoMember(10)] [ValueDropdown("@OdinDropdownHelper.GetAbilities()")]
        public string AbilityName;
        protected override void Execute(Entity applier, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            var ac = target.GetComponent<AbilityComponent>();
            if (ac != null)
            {
                var config = ConfigAbilityCategory.Instance.Get(AbilityName);
                if (config != null)
                    ac.AddAbility(config);
            }
        }
    }
}