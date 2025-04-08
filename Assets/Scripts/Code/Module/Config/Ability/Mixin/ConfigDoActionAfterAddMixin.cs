using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 监听添加后
    /// </summary>
    [NinoType(false)][LabelText("ability或modify添加后DoAction")]
    public partial class ConfigDoActionAfterAddMixin : ConfigAbilityMixin
    {
        [NinoMember(1)][LabelText("Actions:初始Action目标(Target)为Applier(Modify或Ability持有者)")]
        public ConfigAbilityAction[] Actions;
    }
}