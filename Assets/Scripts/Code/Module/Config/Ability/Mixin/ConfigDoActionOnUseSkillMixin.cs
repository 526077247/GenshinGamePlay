using System.Collections.Generic;
using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 监听输入
    /// </summary>
    [NinoType(false)][LabelText("监听使用技能DoAction")]
    public partial class ConfigDoActionOnUseSkillMixin: ConfigAbilityMixin
    {
        [NinoMember(1)]
        public int SkillId;
        [NinoMember(2)]
        public ConfigAbilityAction[] Actions;
    }
}