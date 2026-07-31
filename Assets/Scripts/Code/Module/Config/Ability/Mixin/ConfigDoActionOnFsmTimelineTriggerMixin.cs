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
    [ProtoContract][LabelText("监听FsmTimeline触发事件DoAction")]
    public partial class ConfigDoActionOnFsmTimelineTriggerMixin: ConfigAbilityMixin
    {
        [ProtoMember(1)]
        public string TriggerId;
        [ProtoMember(2)]
        public ConfigAbilityAction[] Actions;
    }
}