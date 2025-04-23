using System.Collections.Generic;
using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 监听输入
    /// </summary>
    [NinoType(false)][LabelText("监听输入DoAction")]
    public partial class ConfigDoActionOnInputMixin: ConfigAbilityMixin
    {
        [NinoMember(1)]
        public GameKeyCode KeyCode;
        [NinoMember(2)]
        public bool IgnoreUI;
        [NinoMember(3)]
        public ConfigAbilityAction[] Actions;
    }
}