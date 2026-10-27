using ProtoBuf;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif

namespace TaoTie
{
    /// <summary>
    /// 受击方冲量抗性：按 <see cref="ConfigHitPattern.HitImpulseType"/> 标签缩放外部击退冲量。
    ///
    /// 为什么放在受击方而不是攻击方：同一份攻击打在不同单位上应该有不同反应（史莱姆被打飞、
    /// 石像纹丝不动），这个差异属于「谁在挨打」，不属于「谁在打」。
    ///
    /// 与 <see cref="ConfigActorCommon.CanBlock"/> 的分工：CanBlock 是纯几何开关（能不能被穿过），
    /// 抗性决定「挨打时愿意吃下多少冲量」。木桩那种「完全不动」用抗性 0 表达。
    ///
    /// 口径统一说明：项目已确认<b>不再引入物理质量</b>（原 BodyMass 已删除），
    /// 将来做 P2 的撞击冲量传递时，也按本表的 Scale 作为「被推动系数」分配，
    /// 而不是 m1/m2 的质量比 —— 策划能直观看懂，且不需要为装饰物编造假质量。
    /// </summary>
    [ProtoContract]
    public partial class ConfigHitImpulseResist
    {
        [ProtoMember(1)][LabelText("击退类型")][Tooltip("与 ConfigHitPattern.HitImpulseType 匹配；留空字符串表示「未匹配到具体类型时的兜底值」")]
        public string Type;

        [ProtoMember(2)][LabelText("冲量缩放")][Tooltip("0=完全免疫击退，1=不缩放，>1=被击退得更远")][Range(0f, 3f)]
        public float Scale = 1f;
    }
}
