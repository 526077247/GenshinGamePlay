using ProtoBuf;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 监听调用Execute方法
    /// </summary>
    [ProtoContract][LabelText("ability或modify调用执行时DoAction")]
    public partial class ConfigDoActionByExecuteMixin : ConfigAbilityMixin
    {
        [ProtoMember(1)][LabelText("Actions:初始Action目标(Target)为Applier(Modify或Ability持有者)")]
        public ConfigAbilityAction[] Actions;
    }
}