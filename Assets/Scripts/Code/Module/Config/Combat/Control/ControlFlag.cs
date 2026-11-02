using System;

namespace TaoTie
{
    /// <summary>
    /// 行为能力位。
    /// 1. 新增一种控制（定身/沉默/缴械）只需在配置里填 <see cref="ControlFlag"/> 位，代码零改动 —
    /// 2. 多个控制同时生效时掩码取并集，天然表达"眩晕 + 减速"共存，
    ///    也不需要为每种组合再写一条互斥规则。
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

        /// <summary>使用道具</summary>
        UseItem = 1 << 4,

        /// <summary>被选为攻击目标（潜行/虚体用；刻意不并入 <see cref="CrowdControl"/></summary>
        BeSelected = 1 << 5,

        /// <summary>
        /// 常规硬控集合：眩晕/击飞这一档剥夺的全部能力。
        /// 不含 <see cref="BeSelected"/> —— "不可被选中"是隐身语义，不该被眩晕顺带赋予。
        /// </summary>
        CrowdControl = Move | Turn | CastSkill | Attack,
    }
}
