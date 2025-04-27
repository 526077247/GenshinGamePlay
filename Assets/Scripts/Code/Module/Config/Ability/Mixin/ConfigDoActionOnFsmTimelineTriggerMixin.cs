using System.Collections.Generic;
using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 监听模型上触发器触发
    /// </summary>
    [NinoType(false)][LabelText("监听FsmTimeline触发事件DoAction")]
    public partial class ConfigDoActionOnFsmTimelineTriggerMixin: ConfigAbilityMixin
    {
        [NinoMember(1)]
        public string TriggerId;
        [NinoMember(2)]
        public ConfigAbilityAction[] Actions;
    }
}