using Nino.Core;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [NinoType(false)]
    public partial class SetGadgetState: ConfigAbilityAction
    {
        [NinoMember(10)]
#if UNITY_EDITOR
        [ValueDropdown("@"+nameof(OdinDropdownHelper)+"."+nameof(OdinDropdownHelper.GetGadgetState)+"()")]
#endif
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