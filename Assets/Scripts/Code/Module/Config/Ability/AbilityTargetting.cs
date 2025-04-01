using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    public enum AbilityTargetting
    {
        [LabelText("Self,自身")]
        Self,
        [LabelText("Caster,Ability持有者")]
        Caster,
        [LabelText("Target,目标")]
        Target,
        [LabelText("SelfAttackTarget,战斗状态目标列表")]
        SelfAttackTarget,
        [LabelText("ModifierApplier,Modifier施加者*")][Tooltip("非Modifier则为空")]
        Applier,
        [LabelText("CurLocalAvatar,玩家自身")]
        CurLocalAvatar,
        [LabelText("Other,其他更多")]
        Other,
    }
}