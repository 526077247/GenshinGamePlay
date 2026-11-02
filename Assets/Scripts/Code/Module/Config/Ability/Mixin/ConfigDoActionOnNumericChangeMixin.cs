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
    /// 监听数值变化，满足比较条件时执行 Actions。
    ///
    /// 用来替代「某个机制必须在 C# 里写死触发点」的做法：韧性归零、血量低于 30%、怒气攒满
    /// 这类事件都只是「某个数值跨过了某个阈值」，配置出来即可，不必再为一个机制新增一段代码。
    /// </summary>
    [ProtoContract][LabelText("数值变化时DoAction")]
    public partial class ConfigDoActionOnNumericChangeMixin : ConfigAbilityMixin
    {
        [ProtoMember(1)][LabelText("*数值类型")]
        [Tooltip("填 NumericType 的 id，如 1012(EN)。填派生段(如 10121)也能用，运行时会被归一化到基础段——事件只在基础段广播")]
        public int NumericType;

        [ProtoMember(2)][LabelText("比较符")]
        [Tooltip("默认 ==。注意这里不能写非零初始化值：protobuf 只按 default(T) 判断写不写，初始化值不参与")]
        public CompareMode Compare;

        [ProtoMember(3)][LabelText("比较值")]
        public float Value;

        [ProtoMember(4)][LabelText("触发时机")]
        public NumericTriggerMode Mode = NumericTriggerMode.FallingEdge;

        [ProtoMember(5)][LabelText("挂载时已满足则立即触发一次")]
        [Tooltip("默认关闭。开启后「出生时韧性就是 0」的单位会一出生就触发，通常不是想要的")]
        public bool TriggerOnInit;

        [ProtoMember(6)][LabelText("额外条件")]
        public ConfigAbilityPredicate Predicate;

        [ProtoMember(7)][LabelText("Actions:初始Action目标(Target)为Applier(Modify或Ability持有者)")]
        public ConfigAbilityAction[] Actions;
    }
    
    /// <summary>
    /// 数值监听的触发时机。
    ///
    /// 「当前值满足条件」和「越过条件」是两件事：破韧要的是后者（从有韧到没韧的那一下），
    /// 若只看当前值，任何一次把值写成 0 的操作都会误触发，而一次扣穿到负数又会漏触发。
    ///
    /// 注意 <see cref="FallingEdge"/> 必须是 0：protobuf 只按 default(T) 判断"要不要写"，
    /// 若它拿 1 且带初始化器，配置成 <see cref="Level"/>(0) 时会被当成默认值省略，
    /// 读回来又变回 1，等于配了不生效。
    /// </summary>
    public enum NumericTriggerMode
    {
        [LabelText("跨入条件(如韧性归零)")]
        FallingEdge = 0,
        [LabelText("当前值满足条件")]
        Level = 1,
        [LabelText("跨出条件(如韧性回满)")]
        RisingEdge = 2,
        [LabelText("任意变化")]
        AnyChange = 3,
    }
}
