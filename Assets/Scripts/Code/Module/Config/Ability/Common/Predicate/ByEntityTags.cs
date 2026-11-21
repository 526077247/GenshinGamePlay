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
    /// 实体标记条件：判断定向到的实体是否挂着指定 <see cref="GameplayTags"/>。
    ///
    /// <b>与 BySpecialCompare + TagStackValue 的区别</b>：本谓词读的是 <see cref="ControlComponent.Tags"/>
    /// 的"有/无、组合命中"，语义精确对应"是否处于某状态"，不受 <see cref="TagStackValue.Min"/> 默认 1
    /// 的兜底值干扰；读层数走 <see cref="MinStack"/>（此时 Tags 必须是单个原子标记）。
    ///
    /// <b>定向靠基类 <see cref="ConfigAbilityPredicate.Target"/></b>：攻击钩子里 Action 的初始 target
    /// 是受击者，配 <c>Target = AbilityTargetting.Target</c> 即读受击者身上的标记（"借用"姿势）。
    /// 基类 Target 默认是 <c>Self</c>，漏填会读施法者自身 —— 那是稳定的假，务必显式填 Target。
    /// </summary>
    [ProtoContract]
    [LabelText("实体标记条件")]
    public partial class ByEntityTags : ConfigAbilityPredicate
    {
        /// <summary>要检查的标记。单个原子标记或组值；组值按 <see cref="Match"/> 决定任一/全命中。</summary>
        [ProtoMember(10)][LabelText("*要检查的标记")]
        [Tooltip("单个原子标记或组值。组值按匹配方式决定 任一/全部")]
        public GameplayTags Tags = GameplayTags.None;

        /// <summary>组值时：任一命中（Any）还是全部命中（All）。单标记时两者等价。</summary>
        [ProtoMember(11)][LabelText("匹配方式")]
        [Tooltip("Tags 填了多个位时：任一命中 / 全部命中。单个标记时两者等价")]
        public TagMatchMode Match = TagMatchMode.Any;

        /// <summary>
        /// 最低层数。0 = 只看 tag 有无（走 HasTag/HasAllTags，支持组值）；
        /// &gt;0 = 改走 <see cref="ControlComponent.GetTagStack"/>，此时 <see cref="Tags"/> 必须是单个原子标记。
        /// </summary>
        [ProtoMember(12)][LabelText("最低层数(0=只看有无)")]
        [Tooltip("0=只看标记有无；>0=按层数比较，此时 Tags 只能是单个原子标记（组值会判为不满足）")]
        public int MinStack;

        public override bool Evaluate(Entity actor, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            if (Tags == GameplayTags.None) return false;

            using (var entities = TargetHelper.ResolveTarget(actor, ability, modifier, target, Target))
            {
                if (entities.Count == 0) return false;
                var control = entities[0].GetComponent<ControlComponent>();
                if (control == null) return false;

                if (MinStack > 0)
                {
                    // 层数模式只接受单个原子标记：GetTagStack 遇组合值会报错，这里先拦成"不满足"。
                    return GameplayTagMeta.IsSingleTag(Tags) && control.GetTagStack(Tags) >= MinStack;
                }

                return Match == TagMatchMode.All ? control.HasAllTags(Tags) : control.HasTag(Tags);
            }
        }
    }

    /// <summary>组值标记的匹配方式。</summary>
    public enum TagMatchMode
    {
        /// <summary>命中任意一个位即为真。</summary>
        [LabelText("任一命中")] Any = 0,
        /// <summary>全部位都命中才为真。</summary>
        [LabelText("全部命中")] All = 1,
    }
}
