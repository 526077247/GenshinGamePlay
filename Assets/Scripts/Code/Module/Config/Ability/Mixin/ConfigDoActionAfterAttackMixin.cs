using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 监听攻击后
    /// </summary>
    [NinoType(false)][LabelText("攻击后DoAction")]
    public partial class ConfigDoActionAfterAttackMixin: ConfigAbilityMixin
    {
        [NinoMember(1)][LabelText("Actions:初始Action目标(Target)为受击者")]
        public ConfigAbilityAction[] Actions;
    }
}