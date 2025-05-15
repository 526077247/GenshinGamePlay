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
#if UNITY_EDITOR
        [ValueDropdown("@"+nameof(GameKeyCode)+"."+nameof(GameKeyCode.GetGameKeyCodeList)+"()")]
#endif
        public int KeyCode;
        [NinoMember(2)][LabelText("*UI交互时忽略")][Tooltip("勾选时,PC鼠标或移动端点击在UI上时忽略(如果本身就是UI按钮触发的输入则不受影响)")]
        public bool IgnoreUI;
        [NinoMember(3)]
        public ConfigAbilityAction[] Actions;
    }
}