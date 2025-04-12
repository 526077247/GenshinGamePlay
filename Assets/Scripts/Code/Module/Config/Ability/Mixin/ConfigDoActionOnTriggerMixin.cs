using System.Collections.Generic;
using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 监听触发器触发
    /// </summary>
    [NinoType(false)][LabelText("监听触发器触发事件DoAction")]
    public partial class ConfigDoActionOnTriggerMixin: ConfigAbilityMixin
    {
        [NinoMember(1)][LabelText("TriggerEnterActions:初始Action目标(Target)为进入触发器的Entity")]
        public ConfigAbilityAction[] TriggerEnterActions;
        [NinoMember(2)][LabelText("TriggerExitActions:初始Action目标(Target)为离开触发器的Entity")]
        public ConfigAbilityAction[] TriggerExitActions;
        // [NinoMember(3)][LabelText("Actions:初始Action目标(Target)为保持在触发器的Entity(每物理帧触发一次)")]
        // public ConfigAbilityAction[] TriggerStayActions;
    }
}