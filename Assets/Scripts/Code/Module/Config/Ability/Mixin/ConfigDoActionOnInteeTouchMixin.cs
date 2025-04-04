﻿using Nino.Core;
using Sirenix.OdinInspector;

namespace TaoTie
{
    /// <summary>
    /// 监听点击面板按钮
    /// </summary>
    [NinoType(false)][LabelText("点击面板按钮DoAction")]
    public partial class ConfigDoActionOnInteeTouchMixin : ConfigAbilityMixin
    {
        [NinoMember(1)]
        public ConfigAbilityAction[] Actions;

        [NinoMember(2)]
        public int LocalId;
    }
}