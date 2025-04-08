using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 受到攻击后
    /// </summary>
    [NinoType(false)][LabelText("当受到攻击后DoAction")]
    public partial class ConfigDoActionAfterBeAttackMixin: ConfigAbilityMixin
    {
        [NinoMember(1)][LabelText("Actions:初始Action目标(Target)为攻击者")]
        public ConfigAbilityAction[] Actions;
    }
}