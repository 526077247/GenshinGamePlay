using System.Collections.Generic;
using ProtoBuf;
using Sirenix.OdinInspector;
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