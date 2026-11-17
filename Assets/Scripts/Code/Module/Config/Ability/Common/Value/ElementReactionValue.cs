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
    /// 元素反应的哪个数值。
    /// </summary>
    public enum ElementReactionField
    {
        [LabelText("反应Id")] ReactionId = 0,
        [LabelText("触发元素")] TriggerElement = 1,
        [LabelText("被撞元素")] OtherElement = 2,
        [LabelText("触发元素扣除量")] TriggerConsumed = 3,
        [LabelText("被撞元素扣除量")] OtherConsumed = 4,
    }

    /// <summary>
    /// 读宿主<b>最近一次元素反应</b>的某个数值（<see cref="ElementComponent.LastReactionId"/> 那一组）。
    ///
    /// 存在的理由：反应效果写在 <see cref="ConfigElementReactionMixin"/> 的 Actions 里，
    /// 而事件载荷不会进 Actions 的求值上下文，所以数值要能从宿主上读回来。
    /// 事件与属性一一对应，读到的一定是刚触发的那一次（同一帧内不会被后续反应覆盖 —— 除非
    /// 本 Mixin 的 Actions 自己又引发了反应，那种情况由 Mixin 的重入保护挡掉）。
    ///
    /// 宿主没有元素组件 / 还没发生过反应时返回 0。
    /// </summary>
    [ProtoContract]
    [LabelText("元素反应数值")]
    public partial class ElementReactionValue : BaseValue
    {
        [ProtoMember(1)][LabelText("*取值")]
        public ElementReactionField Field;

        public override float Resolve(Entity entity, ActorAbility ability, ActorModifier modifier = null)
        {
            if (entity == null || entity.IsDispose) return 0;
            var element = entity.GetComponent<ElementComponent>();
            if (element == null) return 0;

            switch (Field)
            {
                case ElementReactionField.ReactionId: return element.LastReactionId;
                case ElementReactionField.TriggerElement: return element.LastReactionTrigger;
                case ElementReactionField.OtherElement: return element.LastReactionOther;
                case ElementReactionField.TriggerConsumed: return element.LastReactionTriggerConsumed;
                case ElementReactionField.OtherConsumed: return element.LastReactionOtherConsumed;
                default: return 0;
            }
        }
    }
}
