﻿using System.Collections.Generic;
using Nino.Core;
using Sirenix.OdinInspector;

namespace TaoTie
{
    /// <summary>
    /// GameObject加载完成后触发
    /// </summary>
    [NinoType(false)][LabelText("GameObject加载完成后触发")]
    public partial class ConfigDoActionAfterLoadObjectMixin: ConfigAbilityMixin
    {
        [NinoMember(1)]
        public ConfigAbilityAction[] Actions;
    }
}