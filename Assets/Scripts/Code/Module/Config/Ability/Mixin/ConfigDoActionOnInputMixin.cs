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
    /// 监听输入
    /// </summary>
    [ProtoContract][LabelText("监听输入DoAction")]
    public partial class ConfigDoActionOnInputMixin: ConfigAbilityMixin
    {
        [ProtoMember(1)]
#if UNITY_EDITOR
        [ValueDropdown("@"+nameof(GameKeyCode)+"."+nameof(GameKeyCode.GetGameKeyCodeList)+"()")]
#endif
        public int KeyCode;
        [ProtoMember(2)][LabelText("*UI交互时忽略")][Tooltip("勾选时,PC鼠标或移动端点击在UI上时忽略(如果本身就是UI按钮触发的输入则不受影响)")]
        public bool IgnoreUI;
        [ProtoMember(3)]
        public ConfigAbilityAction[] Actions;
    }
}