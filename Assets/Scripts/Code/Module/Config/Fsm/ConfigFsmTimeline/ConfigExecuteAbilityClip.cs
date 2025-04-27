using Nino.Core;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [NinoType(false)]
    public partial class ConfigExecuteAbilityClip:ConfigFsmClip
    {
        [NinoMember(10)]
#if UNITY_EDITOR
        [ValueDropdown("@"+nameof(OdinDropdownHelper)+"."+nameof(OdinDropdownHelper.GetAbilities)+"()",AppendNextDrawer = true)]
#endif
        public string AbilityName;
        [NinoMember(11)][LabelText("当还未开始时被打断是否执行")]
        public bool ExecuteOnBreak;
    }
}