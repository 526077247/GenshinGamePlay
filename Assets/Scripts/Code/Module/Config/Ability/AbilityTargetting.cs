using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    public enum AbilityTargetting
    {
        [LabelText("Self,*自身")][Tooltip("即Action执行者,也是Action所属Modify或Ability持有者")]
        Self,
        [LabelText("Caster,Ability持有者")]
        Caster,
        [LabelText("Target,经过一系列重定向的Action目标")]
        Target,
        [LabelText("SelfAttackTarget,战斗状态的目标")]
        SelfAttackTarget,
        [LabelText("ModifierApplier,*Modifier施加者")][Tooltip("非Modifier执行的则为Ability持有者")]
        Applier,
        [LabelText("CurLocalAvatar,玩家自身")]
        CurLocalAvatar,
        [LabelText("Other,其他更多")]
        Other,
    }
}