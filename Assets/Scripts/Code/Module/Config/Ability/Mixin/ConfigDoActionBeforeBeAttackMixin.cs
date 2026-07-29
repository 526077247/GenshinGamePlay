using ProtoBuf;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 受到攻击前
    /// </summary>
    [ProtoContract][LabelText("受到攻击前DoAction")]
    public partial class ConfigDoActionBeforeBeAttackMixin: ConfigAbilityMixin
    {
        [ProtoMember(1)][LabelText("Actions:初始Action目标(Target)为攻击者")]
        public ConfigAbilityAction[] Actions;
    }
}