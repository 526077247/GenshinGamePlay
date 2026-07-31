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
    /// GadgetState状态改变
    /// </summary>
    [ProtoContract][LabelText("GadgetState状态改变时DoAction")]
    public partial class ConfigDoActionByGadgetStateMixin: ConfigAbilityMixin
    {
        [ProtoMember(1)]
        public List<GadgetState> StateIDs;
        [ProtoMember(2)]
        public ConfigAbilityPredicate EnterPredicate;
        [ProtoMember(3)][LabelText("EnterActions:初始Action目标(Target)为Applier(Modify或Ability持有者)")]
        public ConfigAbilityAction[] EnterActions;
        [ProtoMember(4)]
        public ConfigAbilityPredicate ExitPredicate;
        [ProtoMember(5)][LabelText("ExitActions:初始Action目标(Target)为Applier(Modify或Ability持有者)")]
        public ConfigAbilityAction[] ExitActions;
    }
}