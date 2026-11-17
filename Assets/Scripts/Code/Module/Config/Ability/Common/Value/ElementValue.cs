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
    /// 读<b>宿主实体身上某个元素当前存了多少元素量</b>。
    ///
    /// 这是"配置层自定义元素反应"的核心原料：拿到元素量之后想干什么由配置决定 ——
    /// 拿去 <see cref="ModifyAttackResult.DamageRatioMultiplier"/> 做伤害加成、
    /// 拿去 <see cref="OperatorValue"/> 算"元素量 / 上限"的比例、
    /// 或者先 <see cref="ByElementDurability"/> 判断够不够再 <see cref="ConsumeElement"/> 扣掉。
    ///
    /// <b>宿主由调用方决定，本类不做 Source 分派</b>：
    /// <list type="bullet">
    /// <item>挂在受击钩子（BeforeBeAttack / AfterBeAttack / OnBeingHit）里时，宿主是受击方 ——
    /// <c>ModifyAttackResult</c> 与各攻击 Mixin 都用 <c>target</c> 作求值宿主，天然正确。</item>
    /// <item>挂在施法者自己的 Tick / AfterAdd 里时宿主是施法者，想读别人请显式把 Targetting 指过去。</item>
    /// </list>
    /// 读不到元素组件（木桩 / 子弹 / 特效）时返回 0，不报错 —— 那是常态，不是配置错误。
    /// </summary>
    [ProtoContract]
    [LabelText("元素量")]
    public partial class ElementValue : BaseValue
    {
        [ProtoMember(1)][LabelText("*元素Id")]
        [Tooltip("要读哪个元素的元素量。0 恒返回 0，不要填")]
        public int Element;

        /// <summary>没有元素挂上时返回多少。<b>默认 0</b>（与 <see cref="TagStackValue"/> 的 1 不同）：
        /// 元素量的语义就是"没有就是 0"，而它几乎总被当作乘法的加数而非乘数，
        /// 退回非零会让"没挂元素也吃加成"变成一个只有打出来才看得见的错。</summary>
        [ProtoMember(2)][LabelText("没挂时返回")]
        public float Default;

        public override float Resolve(Entity entity, ActorAbility ability, ActorModifier modifier = null)
        {
            if (entity == null || entity.IsDispose) return Default;
            var element = entity.GetComponent<ElementComponent>();
            if (element == null) return Default;
            float value = element.GetDurability(Element);
            return value > 0 ? value : Default;
        }
    }
}
