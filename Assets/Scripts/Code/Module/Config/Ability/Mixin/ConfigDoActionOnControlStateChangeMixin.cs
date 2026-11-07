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
    /// 监听<b>控制状态</b>变化（<see cref="MessageId.ControlStateChanged"/>），满足触发模式时执行 Actions。
    ///
    /// 这条轴只回答「能力被剥夺成什么样」，配置面刻意<b>没有 WatchFlags 字段</b>：
    /// <see cref="ControlFlag"/> 在文件头写明「引擎内部枚举，绝不出现在任何配置文件里」，
    /// 暴露按能力位过滤会让配置重新获得手写剥夺位的能力，类型系统这层保护就没了。
    /// 想按「眩晕 / 冰冻」这种<b>原因</b>过滤，请用
    /// <see cref="ConfigDoActionOnEntityTagsChangeMixin"/> 监听 <see cref="GameplayTags"/> 段 1。
    ///
    /// <b>已知边界</b>：<see cref="ControlComponent.Rebuild"/> 用 <c>old == deny</c> 门控广播，
    /// 两个不同控制派生出完全相同的剥夺并集时，第二个挂上不会发事件。
    /// 这是「结论轴」的固有语义，不要拿它当「控制附加次数」用 —— 那个请用 Tag 轴。
    /// </summary>
    [ProtoContract]
    [LabelText("控制状态变化时DoAction")]
    public partial class ConfigDoActionOnControlStateChangeMixin : ConfigAbilityMixin
    {
        [ProtoMember(1)][LabelText("触发时机")]
        [Tooltip("默认 Begin。判定走「本次新获得的能力剥夺位」(Gained)，而不是 IsBegin —— " +
                 "吟唱中被眩晕时 OldMask 已经非 None，IsBegin 为 false 但控制确实刚附加。\n" +
                 "• Begin：新剥夺了任一能力\n" +
                 "• End：能力全部恢复（NewMask == None）\n" +
                 "• HardStop：本次进入「五项能力全禁」的完全钉死\n" +
                 "• Any：任意掩码变化（门控保证广播时必然有变化）")]
        public ControlTriggerMode Mode;

        [ProtoMember(2)][LabelText("挂载时已满足则立即触发一次")]
        [Tooltip("默认关闭。开启后，挂载时身上已有剥夺位会立刻执行一次 Actions。")]
        public bool TriggerOnInit;

        [ProtoMember(3)][LabelText("额外条件")]
        public ConfigAbilityPredicate Predicate;

        [ProtoMember(4)][LabelText("Actions:初始目标为Applier")]
        public ConfigAbilityAction[] Actions;
    }

    /// <summary>
    /// 控制状态变化的触发时机。<see cref="Begin"/> 必须是 0 且字段不带初始化器，
    /// 理由同 <see cref="TagTriggerMode"/>（protobuf 按 default(T) 决定是否落库）。
    /// </summary>
    public enum ControlTriggerMode
    {
        [LabelText("新获得剥夺位(控制附加)")]
        Begin = 0,
        [LabelText("全部解除控制")]
        End = 1,
        [LabelText("进入完全钉死")]
        HardStop = 2,
        [LabelText("任意变化")]
        Any = 3,
    }
}
