using ProtoBuf;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [ProtoContract]
    public partial class ConfigExecuteAbilityClip:ConfigFsmClip
    {
        [ProtoMember(10)]
#if UNITY_EDITOR
        [ValueDropdown("@"+nameof(OdinDropdownHelper)+"."+nameof(OdinDropdownHelper.GetAbilities)+"()",AppendNextDrawer = true)]
#endif
        public string AbilityName;
        [ProtoMember(11)][LabelText("当还未开始时被打断是否执行")]
        public bool ExecuteOnBreak;
    }
}