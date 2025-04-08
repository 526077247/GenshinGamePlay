using System.Collections.Generic;
using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// GadgetState状态改变
    /// </summary>
    [NinoType(false)][LabelText("GadgetState状态改变时DoAction")]
    public partial class ConfigDoActionByGadgetStateMixin: ConfigAbilityMixin
    {
        [NinoMember(1)]
        public List<GadgetState> StateIDs;
        [NinoMember(2)]
        public ConfigAbilityPredicate EnterPredicate;
        [NinoMember(3)][LabelText("EnterActions:初始Action目标(Target)为Applier(Modify或Ability持有者)")]
        public ConfigAbilityAction[] EnterActions;
        [NinoMember(4)]
        public ConfigAbilityPredicate ExitPredicate;
        [NinoMember(5)][LabelText("ExitActions:初始Action目标(Target)为Applier(Modify或Ability持有者)")]
        public ConfigAbilityAction[] ExitActions;
    }
}