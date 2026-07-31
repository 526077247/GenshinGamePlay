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
    [ProtoContract][LabelText("监听使用技能DoAction")]
    public partial class ConfigDoActionOnUseSkillMixin: ConfigAbilityMixin
    {
        [ProtoMember(1)]
        public int SkillId;
        [ProtoMember(2)]
        public ConfigAbilityAction[] Actions;
    }
}