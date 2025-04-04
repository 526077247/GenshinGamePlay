﻿using System.Collections.Generic;
using Nino.Core;
using Sirenix.OdinInspector;

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
        [NinoMember(4)]
        public ConfigAbilityAction[] EnterActions;
        [NinoMember(5)]
        public ConfigAbilityPredicate ExitPredicate;
        [NinoMember(6)]
        public ConfigAbilityAction[] ExitActions;
    }
}