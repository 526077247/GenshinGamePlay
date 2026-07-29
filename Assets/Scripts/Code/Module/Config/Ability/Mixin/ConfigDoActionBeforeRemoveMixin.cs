using ProtoBuf;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 监听移除前
    /// </summary>
    [ProtoContract][LabelText("ability或modify移除前DoAction")]
    public partial class ConfigDoActionBeforeRemoveMixin : ConfigAbilityMixin
    {
        [ProtoMember(1)][LabelText("Actions:初始Action目标(Target)为Applier(Modify或Ability持有者)")]
        public ConfigAbilityAction[] Actions;
    }
}