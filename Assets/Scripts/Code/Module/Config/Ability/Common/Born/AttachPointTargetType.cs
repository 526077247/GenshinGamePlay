using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    public enum AttachPointTargetType
    {
        [LabelText("Target,经过一系列重定向的Action目标")]
        Target = 0,
        [LabelText("Self,*自身")][Tooltip("即Action执行者,也是Action所属Modify或Ability持有者")]
        Self = 1,
        [LabelText("Caster,Ability持有者")]
        Caster = 2,
        [LabelText("ModifierApplier,*Modifier施加者")][Tooltip("非Modifier执行的则为Ability持有者")]
        Applier = 3,
        [LabelText("Owner,*所有者")][Tooltip("Self的父Entity,没有父Entity的则为Self")]
        Owner = 4,
    }
}