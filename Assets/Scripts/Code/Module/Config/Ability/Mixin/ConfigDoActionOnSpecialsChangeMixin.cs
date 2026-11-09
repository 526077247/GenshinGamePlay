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
    /// 监视修饰器状态（<c>ModifierSpecials</c> 里声明的那份数据），越过阈值时执行 Actions。
    ///
    /// 它是「破盾 / 充满 / 耗尽」这类事件的标准出口 —— 这些机制不需要各自一段 C#，
    /// 它们都只是「某个状态跨过了某个值」。配好这条 Mixin 即可：
    /// 盾归零时放特效、充能满时换形态、层数耗尽时移除自身。
    ///
    /// <b>与 <see cref="ConfigDoActionOnNumericChangeMixin"/> 的区别只在数据源</b>：
    /// 那个监听 <see cref="NumericComponent"/>（实体级、多来源共用一个池），
    /// 这个监听修饰器实例自己的 states（每个目标、每次施加各一份）。
    /// 所以「每个人各有一面 3 秒盾」这种需求只能走这条。
    ///
    /// <b>为什么不用在每个写入点手挂 Actions</b>：
    /// 状态的写入方可能有很多（tick 回盾、受击扣减、外部削减），
    /// 把「归零了要做什么」绑在写入点上，就得保证每个写入点都记得触发；
    /// 监听状态变化则天然覆盖全部来源。
    /// </summary>
    [ProtoContract][LabelText("修饰器状态变化时DoAction")]
    public partial class ConfigDoActionOnSpecialsChangeMixin : ConfigAbilityMixin
    {
        [ProtoMember(1)][LabelText("*状态名")]
        [Tooltip("要监视的 key，对应 ConfigAbilityModifier.ModifierSpecials 里声明的名字。\n" +
                 "拼错不会报错 —— 只是永远等不到变化，所以运行时会在挂载时检查 key 是否存在并打日志。")]
        public string Key;

        [ProtoMember(2)][LabelText("比较符")]
        [Tooltip("默认 ==（float 用近似比较）。注意这里不能写非零初始化值：protobuf 只按 default(T) 判断写不写")]
        public CompareMode Compare;

        [ProtoMember(3)][LabelText("比较值")]
        public float Value;

        [ProtoMember(4)][LabelText("触发时机")]
        [Tooltip("跨入条件 = 旧值不满足、新值满足的那一瞬间。破盾必须用这个 —— " +
                 "用「当前值满足」的话，任何一次把盾写成 0 的操作都会重复触发")]
        public NumericTriggerMode Mode = NumericTriggerMode.FallingEdge;

        [ProtoMember(5)][LabelText("挂载时已满足则立即触发一次")]
        [Tooltip("默认关闭。开启后「挂上时盾就是 0」会立刻触发一次，通常不是想要的")]
        public bool TriggerOnInit;

        [ProtoMember(6)][LabelText("额外条件")]
        public ConfigAbilityPredicate Predicate;

        [ProtoMember(7)][LabelText("Actions:初始Action目标(Target)为状态持有者")]
        [Tooltip("执行时机在状态写入的同步栈里。要移除自身请用 RemoveModifier / RemoveAbility —— " +
                 "它们内部走帧末延迟，不会在 SetSpecial 还没返回时把字典回池。")]
        public ConfigAbilityAction[] Actions;
    }
}
