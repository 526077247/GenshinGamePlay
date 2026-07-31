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
    /// 监听状态机状态
    /// </summary>
    [ProtoContract][LabelText("状态机状态变化时DoAction")]
    public partial class ConfigDoActionByStateIDMixin: ConfigAbilityMixin
    {
        [ProtoMember(1)]
        public string ChargeLayer;
        [ProtoMember(2)]
        public List<string> StateIDs;
        [ProtoMember(3)]
        public ConfigAbilityPredicate EnterPredicate;
        [ProtoMember(4)][LabelText("EnterActions:初始Action目标(Target)为Applier(Modify或Ability持有者)")]
        public ConfigAbilityAction[] EnterActions;
        [ProtoMember(5)]
        public ConfigAbilityPredicate ExitPredicate;
        [ProtoMember(6)][LabelText("ExitActions:初始Action目标(Target)为Applier(Modify或Ability持有者)")]
        public ConfigAbilityAction[] ExitActions;
    }
}