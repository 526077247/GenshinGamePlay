using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 监听点击面板按钮
    /// </summary>
    [NinoType(false)][LabelText("点击面板按钮DoAction")]
    public partial class ConfigDoActionOnInteeTouchMixin : ConfigAbilityMixin
    {
        [NinoMember(1)][LabelText("Actions:初始Action目标(Target)为Applier(Modify或Ability持有者)")]
        public ConfigAbilityAction[] Actions;

        [NinoMember(2)][LabelText("*面板选项Id")][Tooltip("ConfigActor.Intee.Params.Id")]
        public int LocalId;
    }
}