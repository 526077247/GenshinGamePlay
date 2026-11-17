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
    /// 按标记消耗 modifier 的模式。语义由本枚举表达，层数一律由 <c>CountExpr</c> 给，
    /// 两者各归各的字段 —— 不再用数值的正负 / 零来表达语义。
    ///
    /// <see cref="Clear"/> 是净化 / 驱散：把命中实例的层数全清 0，帧末统一摘除。
    /// 因为 <c>ControlComponent.Rebuild</c> 会跳过 0 层实例，标记聚合在<b>同帧</b>就归零，
    /// 效果等价于「整条移除」，但不必让整条 Action 等一帧。
    /// </summary>
    public enum StackRemoveMode
    {
        [LabelText("消耗 N 层")]
        Consume = 0,

        [LabelText("清空全部")]
        Clear = 1,

        [LabelText("减到只剩 N 层")]
        Keep = 2,
    }
    /// <summary>
    /// 按<b>状态标记</b>消耗目标身上的 modifier（净化 / 引爆 / 逐层衰减的统一入口）。
    ///
    /// <b>三种模式共用一套定位与时序</b>：按标记跨来源、跨 ability 命中全部实例，
    /// 与读侧 <c>TagStackValue</c> 的聚合口径严格对称。
    /// <list type="bullet">
    /// <item><c>Consume</c>：消耗 <c>CountExpr</c> 层（引爆 / 逐层衰减）。</item>
    /// <item><c>Keep</c>：减到只剩 <c>CountExpr</c> 层。</item>
    /// <item><c>Clear</c>：清空全部层数 → 帧末整条摘除，等价于净化 / 驱散。</item>
    /// </list>
    ///
    /// <b>数字同步、摘除帧末</b>：层数与 Properties 立即回退，
    /// <see cref="ControlComponent"/> 的 Rebuild 会跳过 0 层实例，因此 <c>tagMask</c> /
    /// <c>tagStacks</c> / <c>IsControlled</c> 在<b>同一帧</b>就释放 —— 旧版净化整体等一帧，
    /// 同一条 Actions 里后续 Action 读 <c>IsControlled</c> 会拿到旧值，这里没有这个坑。
    /// 归零实例由 <see cref="AbilityComponent.ScheduleFlushPendingRemoves"/> 帧末统一 Dispose，
    /// 只改数字不动链表，所以不会在别人遍历 modifiers、或在 Rebuild 广播栈里改集合。
    ///
    /// 免疫（<see cref="ConfigImmuneMixin"/>）拒绝新的施加，本 Action 清掉已经挂着的状态，两者互补。
    /// </summary>
    [ProtoContract][LabelText("按标记消耗/移除Modifier")]
    public partial class RemoveModifierByTag : ConfigAbilityAction
    {
        [ProtoMember(10)][LabelText("*目标标记")][EnumToggleButtons]
        [Tooltip("必填：跨来源、跨 ability 命中所有挂着这些标记的实例；命中任一位即算命中，不看来源、不限条数。\n" +
                 "• 精确：勾具体标记（眩晕 / 灼烧 …）\n" +
                 "• 整段：勾 AllControlKind 一次覆盖全部控制身份，含 bit 8..15 预留位。\n" +
                 "不带标记的 modifier（纯属性 / 纯行为）不会被命中。")]
        public GameplayTags Tags;

        [ProtoMember(11)][LabelText("目标层数")][ShowIf("@"+nameof(Mode)+"!="+nameof(StackRemoveMode)+"."+nameof(StackRemoveMode.Clear))]
        [Tooltip("Consume=要消耗的层数、Keep=要保留的层数，两者必填；Clear 忽略本字段。" +
                 "可填 OperatorValue 把 TagStackValue(Burn) 乘 0.5，实现消耗当前层数一半")]
        public BaseValue CountExpr;

        [ProtoMember(12)][LabelText("模式")]
        [Tooltip("Consume(默认)=消耗 N 层；Clear=清空全部层数并帧末摘除（净化/驱散）；Keep=减到只剩 N 层。" +
                 "Clear 必须显式填 1 —— 枚举默认值 0 会被 protobuf 省略")]
        public StackRemoveMode Mode;

        [ProtoMember(13)][LabelText("结果写入状态")]
        [Tooltip("把实际消耗层数写进 AbilitySpecials 的这个 key，供同一条 Actions 里后续 Action 按消耗量结算；留空不写。\n" +
                 "读回时 SpecialValue 必须显式填 Target=Ability —— Auto 在有 modifier 的上下文里会优先读修饰器层，读不到会返回 Default")]
        public string ResultKey;

        /// <summary>
        /// 运行时校验只打一次。配置对象是全局共享的，而 Action 可能挂在 Tick Mixin 上高频执行，
        /// 每次都打日志会刷屏；非 ProtoMember 字段不落库，也不会污染 bytes。
        /// </summary>
        private bool warned;

        protected override void Execute(Entity actionExecuter, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            // Action 层目前没有加载期校验入口（ValidateModifier 只查 modifier，
            // ValidateMixins 够不到嵌在各类 Mixin 里的 Actions），所以漏填只能运行时吵一次。
            if (Tags == GameplayTags.None)
            {
                WarnOnce("没填目标标记(None)：这条 Action 什么也不会消耗");
                return;
            }

            GameplayTags unknown = Tags & ~(GameplayTagMeta.KnownMask | GameplayTagMeta.GroupMask);
            if (unknown != GameplayTags.None)
            {
                // 不拦：未知位本来就匹配不到任何 modifier（ValidateModifier 禁止它落库），
                // 继续消耗已命名位是安全的，拦掉反而让整条净化失效
                WarnOnce($"目标标记含未命名的位 {unknown}：预留位不会匹配到任何 modifier，" +
                         "请先在 GameplayTags 里给它起名");
            }

            int count = 0;
            if (Mode != StackRemoveMode.Clear)
            {
                if (CountExpr == null)
                {
                    WarnOnce("Consume / Keep 模式必须填 CountExpr");
                    return;
                }

                // 求值在同步栈里，modifier / target 都还活着；
                // 用 target 作宿主，TagStackValue(Source=Attacker) 才读得到要减层的那个实体。
                count = (int)CountExpr.Resolve(target, ability, modifier);
            }

            var ac = target.GetComponent<AbilityComponent>();
            if (ac == null) return;

            int removed = ac.ConsumeStacks(Tags, count, Mode);

            // 结果回写必须在同步栈里：这是「按实际消耗层数结算伤害」的唯一数据来源。
            if (!string.IsNullOrEmpty(ResultKey) && ability != null && !ability.IsDispose)
            {
                ability.SetSpecial(ResultKey, removed);
            }

            if (removed > 0) ac.ScheduleFlushPendingRemoves();
        }

        private void WarnOnce(string reason)
        {
            if (warned) return;
            warned = true;
            Log.Warning($"[GameplayTag] RemoveModifierByTag {reason}");
        }
    }
}
