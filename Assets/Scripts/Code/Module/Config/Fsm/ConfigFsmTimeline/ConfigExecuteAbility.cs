using Nino.Serialization;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigExecuteAbility:ConfigFsmClip
    {
        [NinoMember(10)][ValueDropdown("@OdinDropdownHelper.GetAbilities()")]
        public string AbilityName;
        public override FsmClip CreateClip(FsmState state)
        {
            var res = ExecuteAbilityAction.Create();
            res.OnInit(state,this);
            return res;
        }
    }
}