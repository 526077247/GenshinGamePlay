using System.Collections.Generic;
using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// GameObject加载完成后触发
    /// </summary>
    [NinoType(false)][LabelText("GameObject加载完成后触发")]
    public partial class ConfigDoActionAfterLoadObjectMixin: ConfigAbilityMixin
    {
        [NinoMember(1)][LabelText("Actions:初始Action目标(Target)为Applier(Modify或Ability持有者)")]
        public ConfigAbilityAction[] Actions;
    }
}