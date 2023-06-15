using Nino.Serialization;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigExecuteAbility:ConfigFsmClip
    {
        [NinoMember(10)]
        [ValueDropdown("@"+nameof(OdinDropdownHelper)+"."+nameof(OdinDropdownHelper.GetAbilities)+"()",AppendNextDrawer = true)]
        public string AbilityName;
        [NinoMember(11)]
        public bool ExecuteOnBreak;
        public override FsmClip CreateClip(FsmState state)
        {
            var res = ExecuteAbilityAction.Create();
            res.OnInit(state,this);
            return res;
        }
    }
}