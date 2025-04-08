using System.Collections.Generic;
using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 监听状态机状态
    /// </summary>
    [NinoType(false)][LabelText("状态机状态变化时DoAction")]
    public partial class ConfigDoActionByStateIDMixin: ConfigAbilityMixin
    {
        [NinoMember(1)]
        public string ChargeLayer;
        [NinoMember(2)]
        public List<string> StateIDs;
        [NinoMember(3)]
        public ConfigAbilityPredicate EnterPredicate;
        [NinoMember(4)][LabelText("EnterActions:初始Action目标(Target)为Applier(Modify或Ability持有者)")]
        public ConfigAbilityAction[] EnterActions;
        [NinoMember(5)]
        public ConfigAbilityPredicate ExitPredicate;
        [NinoMember(6)][LabelText("ExitActions:初始Action目标(Target)为Applier(Modify或Ability持有者)")]
        public ConfigAbilityAction[] ExitActions;
    }
}