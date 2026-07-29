using System.Collections.Generic;
using ProtoBuf;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// GameObject加载完成后触发
    /// </summary>
    [ProtoContract][LabelText("GameObject加载完成后触发")]
    public partial class ConfigDoActionAfterLoadObjectMixin: ConfigAbilityMixin
    {
        [ProtoMember(1)][LabelText("Actions:初始Action目标(Target)为Applier(Modify或Ability持有者)")]
        public ConfigAbilityAction[] Actions;
    }
}