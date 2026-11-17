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
    /// 读<b>某个元素的耐久上限</b>（配置值，不是运行时值）。
    ///
    /// 存在的理由只有一个：让配置层能写"元素量占上限的比例"这类判据 ——
    /// <c>OperatorValue{ Left = ElementValue(火), Op = Div, Right = ElementMaxValue(火) }</c>
    /// 再配 <see cref="BySpecialCompare"/> 或 <c>ByNumericCompare</c> 判 "&gt;= 0.8"。
    /// 没有它，所有阈值都只能写死绝对值，改一次元素上限就要把所有反应阈值重配一遍。
    ///
    /// 上限来自 <see cref="ElementConfig.MaxDurability"/>。宿主没有元素组件时返回 0，
    /// 所以拿它作除数前请自己判一下 —— 除零在 <see cref="OperatorValue"/> 里会得到 Infinity。
    /// </summary>
    [ProtoContract]
    [LabelText("元素上限")]
    public partial class ElementMaxValue : BaseValue
    {
        [ProtoMember(1)][LabelText("*元素Id")]
        public int Element;

        public override float Resolve(Entity entity, ActorAbility ability, ActorModifier modifier = null)
        {
            if (entity == null || entity.IsDispose) return 0;
            var element = entity.GetComponent<ElementComponent>();
            if (element == null) return 0;
            return element.GetMaxDurability(Element);
        }
    }
}
