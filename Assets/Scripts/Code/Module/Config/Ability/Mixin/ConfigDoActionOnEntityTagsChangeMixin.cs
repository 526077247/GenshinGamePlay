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
    /// 监听<b>状态标记</b>变化（<see cref="MessageId.EntityTagsChanged"/>），满足触发模式时执行 Actions。
    ///
    /// 「被眩晕了」「灼烧叠了一层」「护盾破了」这类需求都由它表达，不必再在 C# 里加触发点。
    /// 想监听「能力被剥夺成什么样」（能不能动 / 能不能放技能）请用
    /// <see cref="ConfigDoActionOnControlStateChangeMixin"/> —— 两条轴互不相通。
    ///
    /// <b>订阅键是 ControlComponent.Id（组件 Id）而不是实体 Id</b>：
    /// <see cref="ControlComponent.Rebuild"/> 用组件 Id 广播，按实体 Id 订阅永远收不到。
    ///
    /// <b>免疫成功不会产生 Tags 事件</b>：施加在 <c>AbilityComponent.ApplyModifier</c> 入口被整条拒绝，
    /// modifier 实例根本没创建，也就没有 Rebuild。免疫反击请用 <see cref="ConfigImmuneMixin.Actions"/>。
    /// </summary>
    [ProtoContract]
    [LabelText("标记变化时DoAction")]
    public partial class ConfigDoActionOnEntityTagsChangeMixin : ConfigAbilityMixin
    {
        [ProtoMember(1)][LabelText("*监听标记")][EnumToggleButtons]
        [Tooltip("要监听的状态标记。允许组值 AllControlKind / AllActionLock / AllStateFlag —— " +
                 "组值含预留位，以后新增控制标记会自动被覆盖，不用回来改配置。\n" +
                 "必填：留空(None)会导致本条永不触发，加载期会报错。")]
        public GameplayTags WatchTags;

        [ProtoMember(2)][LabelText("触发时机")]
        [Tooltip("默认 Added = 标记集合里新出现了监听位。\n" +
                 "• Added：真正的「附加后」，灼烧叠层(2→3)不触发\n" +
                 "• Removed：标记被摘掉\n" +
                 "• StackChanged：标记集合没变、只是层数涨落\n" +
                 "• Any：新增 / 移除 / 层数变化里，只要与监听标记有关就触发\n" +
                 "• Level：只要当前挂着监听标记就触发（每次变化都成立）\n" +
                 "想同时监听新增和移除，请配两条 Mixin。")]
        public TagTriggerMode Mode;

        [ProtoMember(3)][LabelText("挂载时已满足则立即触发一次")]
        [Tooltip("默认关闭。开启后，Mixin 挂载时身上已经挂着监听标记会立刻执行一次 Actions；" +
                 "此时读到的状态是挂载前的快照。")]
        public bool TriggerOnInit;

        [ProtoMember(4)][LabelText("额外条件")]
        public ConfigAbilityPredicate Predicate;

        [ProtoMember(5)][LabelText("Actions:初始目标为Applier")]
        [Tooltip("事件载荷不带 ApplierId / ModifierName，Actions 的初始 target 只能是宿主自己：" +
                 "「驱散自己」可以，「反击攻击者」做不到。")]
        public ConfigAbilityAction[] Actions;
    }

    /// <summary>
    /// 标记变化的触发时机。
    ///
    /// <b><see cref="Added"/> 必须是 0 且字段不能带非零初始化器</b>：
    /// protobuf 只按 <c>default(T)</c> 决定写不写盘，初始化器不参与。
    /// 若把它挪到 1 并写 <c>Mode = Added</c>，配置默认值得不到落库，
    /// 读回来会变成 0（即另一种语义）—— 这正是 NumericTriggerMode 已经踩过并注明过的坑。
    /// 同理不做成 [Flags]：0 会变成 None，漏填即永不触发。
    /// </summary>
    public enum TagTriggerMode
    {
        [LabelText("新挂上标记(附加后)")]
        Added = 0,
        [LabelText("标记被移除")]
        Removed = 1,
        [LabelText("仅层数变化")]
        StackChanged = 2,
        [LabelText("任意相关变化")]
        Any = 3,
        [LabelText("当前仍挂着就触发")]
        Level = 4,
    }
}
