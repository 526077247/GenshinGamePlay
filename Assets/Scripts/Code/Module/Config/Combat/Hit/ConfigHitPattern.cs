using ProtoBuf;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif
using UnityEngine;

namespace TaoTie
{
    [ProtoContract]
    public partial class ConfigHitPattern
    {
        [ProtoMember(1)][LabelText("击中特效")]
        public string OnHitEffectName;
        [ProtoMember(2)][LabelText("击打力度等级")][BoxGroup("击退信息")]
        public HitLevel HitLevel;
        [ProtoMember(3, IsRequired = true)][NotNull][BoxGroup("击退信息")]
        public BaseValue HitImpulseX = new SingleValue();
        [ProtoMember(4, IsRequired = true)][NotNull][BoxGroup("击退信息")]
        public BaseValue HitImpulseY = new SingleValue();
        [ProtoMember(5)][BoxGroup("击退信息")][LabelText("*击退类型")][Tooltip("击退分类标签。受击方按它查 ConfigActorCommon.HitImpulseResists 决定吃下多少冲量；留空则只命中受击方的兜底项")]
        public string HitImpulseType;
        // 6：原 OverrideHitImpulse（按 HitImpulseType 决定用哪套击退数据）已删除。
        //    那份差异本质是「谁在挨打」，放在攻击配置上永远做不对：同一招打史莱姆和打石像，
        //    该由受击方的 HitImpulseResists 决定，而不是攻击方再维护一份覆盖面。
        //    原实现的触发条件（HitImpulseType 非空 + 填了 OverrideHitImpulse）本身就是个 todo，
        //    全工程配置里 HitImpulseType 恒为空，等于永远走不到。
        //    编号 6 保留不复用。
        [ProtoMember(7)][LabelText("击退来源方向")][BoxGroup("击退信息")]
        public RetreatType RetreatType;
        [ProtoMember(8)][LabelText("击中时停(ms)")][MinValue(0)][BoxGroup("击中时停")]
        public int HitHaltTime;
        [ProtoMember(9)][ShowIf("@"+nameof(HitHaltTime)+">0")][LabelText("时停时间比例")][BoxGroup("击中时停")][Range(0,1)]
        public float HitHaltTimeScale;
        [ProtoMember(10)][ShowIf("@"+nameof(HitHaltTime)+">0")][LabelText("*被格挡时是否时停")][BoxGroup("击中时停")][Tooltip("暂用最终伤害等于0判定为格挡")]
        public bool CanBeDefenceHalt;
        [ProtoMember(11)][LabelText("禁用击中飘字")]
        public bool MuteHitText;
        [ProtoMember(12)][LabelText("支持同目标重复命中")]
        public bool Recurring;
    }
}