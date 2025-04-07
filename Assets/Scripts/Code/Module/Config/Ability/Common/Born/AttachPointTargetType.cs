using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    public enum AttachPointTargetType
    {
        [LabelText("Target,目标")]
        Target = 0,
        [LabelText("Self,自身")]
        Self = 1,
        [LabelText("Caster,Ability持有者")]
        Caster = 2,
        [LabelText("*ModifierApplier,Modifier施加者")][Tooltip("非Modifier则为空")]
        Applier = 3
    }
}