using System.Collections.Generic;
using ProtoBuf;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 监听模型上触发器触发
    /// </summary>
    [ProtoContract][LabelText("监听触发器触发事件DoAction")]
    public partial class ConfigDoActionOnColliderBoxMixin: ConfigAbilityMixin
    {
        [ProtoMember(1)][LabelText("TriggerEnterActions:初始Action目标(Target)为进入触发器的Entity")]
        public ConfigAbilityAction[] TriggerEnterActions;
        [ProtoMember(2)][LabelText("TriggerExitActions:初始Action目标(Target)为离开触发器的Entity")]
        public ConfigAbilityAction[] TriggerExitActions;
        // [ProtoMember(3)][LabelText("Actions:初始Action目标(Target)为保持在触发器的Entity(每物理帧触发一次)")]
        // public ConfigAbilityAction[] TriggerStayActions;
    }
}