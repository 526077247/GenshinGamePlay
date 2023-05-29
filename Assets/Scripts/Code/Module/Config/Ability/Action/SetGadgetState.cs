using Nino.Serialization;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [NinoSerialize]
    public partial class SetGadgetState: ConfigAbilityAction
    {
        [NinoMember(10)]
        [ValueDropdown("@"+nameof(OdinDropdownHelper)+"."+nameof(OdinDropdownHelper.GetGadgetState)+"()")]
        public GadgetState GadgetState;
        protected override void Execute(Entity applier, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            var gc = target.GetComponent<GadgetComponent>();
            if (gc != null)
            {
                gc.SetGadgetState(GadgetState);
            }
        }
    }
}