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
    /// 元素量比较：读目标身上某元素的当前元素量，与阈值比较。
    ///
    /// 这是"配置层自定义反应"的判断入口 —— 框架层<b>不会</b>因为两个元素撞一起就自动放反应，
    /// 而是由配置写成：<c>ByElementDurability{水, >=, 20}</c> + <see cref="ConsumeElement"/> +
    /// <see cref="ModifyAttackResult"/> 这样一条 Actions。想加反应只加配置，不改代码。
    ///
    /// <b>要判"有没有挂"就把阈值填 0 且用 &gt;</b>；元素量恒 &gt;= 0，所以 <c>&gt; 0</c> 等价于"挂着"。
    /// 想判"够不够满"请配 <c>OperatorValue{ElementValue, Div, ElementMaxValue}</c> 走 <c>BySpecialCompare</c>，
    /// 不要在阈值里写死绝对值（改元素上限时会全部失效）。
    ///
    /// 基类 <c>Target</c> 在本类里只作为"例外时的求值宿主"：正常走 Targetting=Target，
    /// 此时传入的就是被重定向后的那个实体，与 Action 的目标一致。
    /// </summary>
    [ProtoContract]
    [LabelText("元素量比较")]
    public partial class ByElementDurability : ConfigAbilityPredicate
    {
        [ProtoMember(10)][LabelText("*元素Id")]
        [Tooltip("要查的元素。0 恒为真值 0，一般配 > 0 会恒假，别填")]
        public int Element;

        [ProtoMember(11)][LabelText("比较符")]
        [Tooltip("默认 ==。不要给这个字段写非零初始化值，否则配 ==(0) 时会被序列化器当成默认值省略")]
        public CompareMode Compare;

        [ProtoMember(12)][LabelText("阈值（元素量）")]
        public float Threshold;

        public override bool Evaluate(Entity actor, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            var ctx = target ?? actor;
            if (ctx == null || ctx.IsDispose) return false;

            var element = ctx.GetComponent<ElementComponent>();
            if (element == null) return false; // 没有元素体系的实体（木桩 / 子弹 / 特效）视为"没挂元素"

            float value = element.GetDurability(Element);
            switch (Compare)
            {
                case CompareMode.Equal:
                    return Mathf.Approximately(value, Threshold);
                case CompareMode.NotEqual:
                    return !Mathf.Approximately(value, Threshold);
                case CompareMode.Greater:
                    return value > Threshold;
                case CompareMode.Less:
                    return value < Threshold;
                case CompareMode.LEqual:
                    return value <= Threshold;
                case CompareMode.GEqual:
                    return value >= Threshold;
                default:
                    return false;
            }
        }
    }
}
