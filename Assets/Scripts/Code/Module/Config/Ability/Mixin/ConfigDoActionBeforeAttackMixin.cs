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
    /// 攻击前
    /// </summary>
    [ProtoContract][LabelText("攻击前DoAction")]
    public partial class ConfigDoActionBeforeAttackMixin: ConfigAbilityMixin
    {
        [ProtoMember(1)][LabelText("Actions:初始Action目标(Target)为受击者")]
        public ConfigAbilityAction[] Actions;
    }
}