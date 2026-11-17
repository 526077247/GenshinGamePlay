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
    /// 元素反应监听：宿主身上发生元素反应时（<see cref="MessageId.ElementReaction"/>）执行 Actions。
    ///
    /// 这是「自动反应」的<b>效果端</b>：元素组件只负责判定能不能反应、按表扣元素、广播事件，
    /// 反应到底造成多少伤害、给谁增伤，全部由这条 Mixin 配的 Actions 决定 ——
    /// 所以「谁和谁反应、怎么反应」仍然是配置说了算，框架不认识任何具体元素。
    ///
    /// 用法：给需要吃反应的实体（通常是受击方）挂一条 Ability + Modifier，Modifier 里加这条 Mixin，
    /// Actions 里用 <see cref="ElementReactionValue"/> 读反应 Id / 双方元素 / 实际扣除量。
    ///
    /// 注意 Actions 的执行目标默认是<b>宿主自己</b>（受击方）——事件载荷不带施法者 Id，
    /// 所以「对攻击者反击」需要自己再用 TriggerAttackEvent 之类的手段传递。
    /// </summary>
    [ProtoContract]
    [LabelText("元素反应时DoAction")]
    public partial class ConfigElementReactionMixin : ConfigAbilityMixin
    {
        [ProtoMember(1)][LabelText("监听反应")]
        [Tooltip("填反应表的 Id（ElementReaction）只监听这一条；填 0 = 任意反应都触发。\n" +
                 "想对每种反应做不同处理，就配多条 Mixin 各管一条。")]
        public int ReactionId;

        [ProtoMember(2)][LabelText("额外条件")]
        public ConfigAbilityPredicate Predicate;

        [ProtoMember(3)][LabelText("Actions:初始目标为宿主")]
        [Tooltip("反应发生后要执行的动作。用 ElementReactionValue 读 ReactionId / 触发元素 / 对方元素 / 双方实际扣除量。")]
        public ConfigAbilityAction[] Actions;
    }
}
