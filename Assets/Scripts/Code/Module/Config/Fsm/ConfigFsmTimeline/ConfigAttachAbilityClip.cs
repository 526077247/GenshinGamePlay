using ProtoBuf;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif

namespace TaoTie
{
    [ProtoContract]
    public partial class ConfigAttachAbilityClip:ConfigFsmClip
    {
        [ProtoMember(10)]
#if UNITY_EDITOR
        [ValueDropdown("@"+nameof(OdinDropdownHelper)+"."+nameof(OdinDropdownHelper.GetAbilities)+"()",AppendNextDrawer = true)]
#endif
        public string AbilityName;
        [ProtoMember(11)][LabelText("当还未开始时被打断是否添加")]
        public bool AddOnBreak;
        [ProtoMember(12)][LabelText("结束时是否移除")]
        public bool RemoveOnOver;
    }
}