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
    /// 标记层数：读"某个 <see cref="GameplayTags"/> 现在一共叠了几层"。
    ///
    /// <b>它不新增任何运行时记账</b>，只是给 <see cref="ControlComponent"/> 里那张现成的
    /// 聚合表开一个只读口（<see cref="ControlComponent.GetTagStack"/>）。
    /// 那个数组在 modifier 增删 / 叠层时由 <c>Rebuild</c> 全量重算、
    /// 把**所有带同一标记的 modifier** 的 StackCount 求和，所以：
    ///
    /// 1. <b>多来源共享一个池</b>：火法叠 3 层 + 环境火 2 层 → 读到 5。
    ///    这正是 <see cref="GameplayTagMeta.MaxStackOf"/> 的设计意图（上限属于标记，不属于 modifier）。
    /// 2. <b>Stack / Multiple 都对</b>：<c>StackingType.Multiple</c> 每个实例的 StackCount 恒为 1，
    ///    读单个 modifier 会永远得到 1；读聚合值才是玩家看到的层数。
    /// 3. <b>天然封顶，不要在这里再 clamp 一次</b>：挂载时 <c>HasStackRoom</c> → <c>GetTagRoom</c>
    ///    已经按 <c>MaxStackOf - GetTagStack</c> 拦过，返回值域就是 [0, 上限]。
    ///    想改上限去改 <see cref="GameplayTagMeta"/>，改配置是没用的。
    ///
    /// 用法（灼烧每跳伤害 = 攻击 × 层数）：
    /// <c>OperatorValue{ Left = NumericValue(ATK), Op = Mul, Right = TagStackValue{ Tag = Burn } }</c>；
    /// 需要"每层线性加成"就写 <c>1 + (层数-1) × k</c> —— 注意 <see cref="OperatorValue"/> 没有
    /// Max/Clamp 节点，缺标记时只能靠本类的 <see cref="Min"/> 兜底。
    /// </summary>
    [ProtoContract]
    [LabelText("标记层数")]
    public partial class TagStackValue : BaseValue
    {
        /// <summary>
        /// 要查的标记。<b>只能是单个原子标记</b>：组合值没有"层"的概念
        /// （求和还是取最大都是拍脑袋），<see cref="ControlComponent.GetTagStack"/> 会直接报错。
        /// 注意 json 里存的是位值数字（Burn = 16777216），抄的时候别抄错位。
        /// </summary>
        [ProtoMember(1)][LabelText("标记")][Tooltip("单个原子标记，如 Burn / Poison / Bleed")]
        public GameplayTags Tag = GameplayTags.None;

        /// <summary>查谁的层数，见 <see cref="StackSource"/>。DoT 用 ModifierOwner。</summary>
        [ProtoMember(2)][LabelText("层级来源")]
        public StackSource Source = StackSource.ModifierOwner;

        /// <summary>
        /// 查不到 / 层数不足时返回多少。<b>默认 1，不是 0。</b>
        ///
        /// 理由：0 一旦进了 <see cref="LogicMode.Mul"/> 就把伤害静默乘成 0，
        /// 而伤害管线最后有 <c>Mathf.Max(0, …)</c>，负数和 0 都没有任何日志 ——
        /// "配了层数倍率但打不出伤害"会变成最难查的那类问题。
        /// 反过来，默认 1 的失败是**可见的**（没叠层也按 1 层算，数值偏大，测一次就能发现）；
        /// 宁可默认值错得显眼，也不要错得安静。
        /// 确实需要"没标记就不生效"（0 伤害）时，把这里显式填 0。
        /// </summary>
        [ProtoMember(3)][LabelText("最少返回")][MinValue(0)]
        public int Min = 1;

        /// <summary>
        /// 只吵一次的开关。求值发生在 DoT 的每跳、每次命中上，
        /// 每帧刷日志比不出日志还糟。配置对象是共享的，所以一次就够定位。
        /// </summary>
        private bool warned;

        public override float Resolve(Entity entity, ActorAbility ability, ActorModifier modifier = null)
        {
            int floor = Min > 0 ? Min : 0;

            if (Tag == GameplayTags.None || !GameplayTagMeta.IsSingleTag(Tag))
            {
                // 组合值不能查层数。这里先拦一次，别让下游 GetTagStack 每跳刷一条 Error
                Warn(ability, $"Tag 必须是单个原子标记，当前 {Tag}({(long) Tag})");
                return floor;
            }

            var owner = ResolveOwner(entity, ability, modifier);
            if (owner == null || owner.IsDispose)
            {
                // 这是"没有上下文"，不是"层数是 0"。绝不静默退回 entity —— 那会读到施法者身上
                // 一个永远不存在的标记，表现是稳定的 0，和"真的没叠层"完全分不出来。
                Warn(ability, $"找不到 Source={Source} 对应的实体（modifier 未传或已被回收？）");
                return floor;
            }

            var control = owner.GetComponent<ControlComponent>();
            if (control == null)
            {
                // 木桩 / 特效 / 子弹这类实体没有控制组件，也就不会维护标记池
                Warn(ability, $"实体 {owner.Id} 上没有 ControlComponent，标记层数恒为 0");
                return floor;
            }

            return Mathf.Max(floor, control.GetTagStack(Tag));
        }

        private Entity ResolveOwner(Entity entity, ActorAbility ability, ActorModifier modifier)
        {
            switch (Source)
            {
                case StackSource.ModifierOwner:
                    // 已回收的 modifier 不能用来解引用：池化复用后 Parent 会指向别的组件，
                    // 读出来的是"另一个人身上的层数"，比读不到更糟
                    if (modifier == null || modifier.IsDispose) return null;
                    return modifier.Parent?.GetParent<Entity>();
                case StackSource.Caster:
                    return ability?.Parent?.GetParent<Entity>();
                case StackSource.Attacker:
                    return entity;
                default:
                    return null;
            }
        }

        private void Warn(ActorAbility ability, string reason)
        {
            if (warned) return;
            warned = true;
            Log.Error($"[TagStackValue] {reason}；Ability = {ability?.Config?.AbilityName}");
        }
    }
}
