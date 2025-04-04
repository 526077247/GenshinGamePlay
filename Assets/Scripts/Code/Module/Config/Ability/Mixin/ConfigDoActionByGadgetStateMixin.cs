﻿using System.Collections.Generic;
using Nino.Core;
using Sirenix.OdinInspector;

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
        [NinoMember(3)]
        public ConfigAbilityAction[] EnterActions;
        [NinoMember(4)]
        public ConfigAbilityPredicate ExitPredicate;
        [NinoMember(5)]
        public ConfigAbilityAction[] ExitActions;
    }
}