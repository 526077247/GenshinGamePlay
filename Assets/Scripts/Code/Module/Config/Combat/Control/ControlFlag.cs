using System;

namespace TaoTie
{
    /// <summary>
    /// 行为许可位：实体「能做什么」。<b>引擎内部枚举，绝不出现在任何配置文件里。</b>
    ///
    /// <b>名字叫 Flag，但它不是"控制标记"</b> —— 状态标记一律在 <see cref="GameplayTags"/>。
    /// 这里每一项都是「一种可以被禁止的动作」，不是「一种被施加的状态」：
    /// 眩晕、灼烧、吟唱都<b>不在</b>这个枚举里，它们只是经由 <see cref="GameplayTagMeta.TierOf"/>
    /// 换算成这里的若干位。看到 <c>ControlFlag.Stun</c> 这种写法一定是错的，编译期就会报错。
    ///
    /// 它和 <see cref="GameplayTags"/> 的关系是「结论」与「原因」：
    /// <code>
    /// 配置写   Tags = Stun            （原因：因为被眩晕了）
    /// 引擎算   Deny |= HardStop       （结论：Move|Turn|CastSkill|Attack|Sprint 全禁）
    /// 行为问   CanDo(ControlFlag.Move) （只读结论）
    /// </code>
    ///
    /// <b>为什么不放进 GameplayTags</b>：
    /// 1. 两类数据的生命周期完全不同 —— 能力位是"引擎支持哪些动作分类"，天然封闭、几乎不涨；
    ///    状态标记是要长期膨胀的开放集合。混在一个 enum 里，配置面板会把 Move / CastSkill
    ///    和 Stun / Burn 平铺成同一排按钮，选错不报错，只会让「我禁了移动」和「我挂了眩晕」
    ///    变成两份互相打架的真相；
    /// 2. 拆开后 <see cref="GameplayTags"/> 的 64 位全部归状态使用（控制 16 + 动作锁 8 + 状态 40），
    ///    而且剥夺位在配置层<b>物理上不存在</b> —— 不需要靠加载期校验去兜，
    ///    类型系统直接让"手写能力位"这件事写不出来。
    ///
    /// 位值一旦发布不能改：它已经写进 <see cref="GameplayTagMeta.HardStop"/> 这类常量语义，
    /// 也在 <c>ControlStateEvent</c> 里跨系统传递。新能力一律在尾部追加。
    /// </summary>
    [Flags]
    public enum ControlFlag
    {
        None = 0,

        /// <summary>主动位移：输入 / AI / 移动策略 / 动画根运动</summary>
        Move = 1 << 0,

        /// <summary>主动转向（把目标朝向写入 SceneEntity.Rotation）</summary>
        Turn = 1 << 1,

        /// <summary>发起技能（含瞬发技能与 FSM 技能状态）</summary>
        CastSkill = 1 << 2,

        /// <summary>普通攻击</summary>
        Attack = 1 << 3,

        /// <summary>使用道具（喝药）</summary>
        UseItem = 1 << 4,

        /// <summary>
        /// 被选为攻击目标（潜行 / 虚体用）。
        /// 刻意不并入任何控制档 —— "不可被选中"是隐身语义，不该被眩晕顺带赋予。
        /// </summary>
        BeSelected = 1 << 5,

        /// <summary>
        /// 冲刺 / 突进。
        ///
        /// 与 <see cref="Move"/> 分开，是为了让"自身施法"和"被硬控"能区别对待：
        /// 吟唱只锁 Move|CastSkill，冲刺仍然可用 —— 它正是打断吟唱的手段；
        /// 眩晕 / 冰冻才锁到 Sprint，把人钉死。
        /// </summary>
        Sprint = 1 << 6,

        /// <summary>全部能力位（校验 / 遍历用）。</summary>
        All = Move | Turn | CastSkill | Attack | UseItem | BeSelected | Sprint,
    }
}
