using Nino.Core;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [NinoType(false)]
    public partial class ConfigAttachAbility:ConfigFsmClip
    {
        [NinoMember(10)]
#if UNITY_EDITOR
        [ValueDropdown("@"+nameof(OdinDropdownHelper)+"."+nameof(OdinDropdownHelper.GetAbilities)+"()",AppendNextDrawer = true)]
#endif
        public string AbilityName;
        [NinoMember(11)][LabelText("当还未开始时被打断是否添加")]
        public bool AddOnBreak;
        [NinoMember(12)][LabelText("结束时是否移除")]
        public bool RemoveOnOver;
        public override FsmClip CreateClip(FsmState state)
        {
            var res = AttachAbilityAction.Create();
            res.OnInit(state,this);
            return res;
        }
    }
}