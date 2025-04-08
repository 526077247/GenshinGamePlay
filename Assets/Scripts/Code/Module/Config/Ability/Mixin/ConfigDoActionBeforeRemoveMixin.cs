using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 监听移除前
    /// </summary>
    [NinoType(false)][LabelText("ability或modify移除前DoAction")]
    public partial class ConfigDoActionBeforeRemoveMixin : ConfigAbilityMixin
    {
        [NinoMember(1)][LabelText("Actions:初始Action目标(Target)为Applier(Modify或Ability持有者)")]
        public ConfigAbilityAction[] Actions;
    }
}