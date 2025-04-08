using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    public enum AbilityTargetting
    {
        [LabelText("Self,*自身")][Tooltip("即Action执行者,也是Action所属Modify或Ability持有者")]
        Self = 0,
        [LabelText("Caster,Ability持有者")]
        Caster = 1,
        [LabelText("Target,经过一系列重定向的Action目标")]
        Target = 2,
        [LabelText("SelfAttackTarget,战斗状态的目标")]
        SelfAttackTarget = 3,
        [LabelText("ModifierApplier,*Modifier施加者")][Tooltip("非Modifier执行的则为Ability持有者")]
        Applier = 4,
        [LabelText("CurLocalAvatar,玩家自身")]
        CurLocalAvatar = 5,
        [LabelText("Owner,*所有者")][Tooltip("Self的父Entity,没有父Entity的则为Self")]
        Owner = 7,
        [LabelText("TargetOwner,*Target所有者")][Tooltip("Target的父Entity,没有父Entity的则为Target")]
        TargetOwner = 8,
        [LabelText("CasterOwner,*Caster所有者")][Tooltip("Caster的父Entity,没有父Entity的则为Caster")]
        CasterOwner = 9,
        
        
        [LabelText("Other,其他更多")]
        Other = 6,
    }
}