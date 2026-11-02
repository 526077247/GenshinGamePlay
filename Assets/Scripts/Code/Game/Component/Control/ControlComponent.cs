using System;

namespace TaoTie
{
    /// <summary>
    /// 控制仲裁者：实体上"能不能动 / 能不能放技能 / 能不能转向"的唯一状态源。
    ///
    /// 取代原先各处 <c>AbilityComponent.IsControlled(类型)</c> 的拉取式轮询。
    /// 控制效果仍然是一个 <see cref="ActorModifier"/>（生命周期、堆叠、多来源语义全部复用，
    /// 不给控制另造一套计时器），但"控制 → 行为许可"的映射只在这里算一次，
    /// 对外出口只有一个 <see cref="CanDo"/>。
    ///
    /// 数据流：
    /// <code>
    /// modifier 增删
    ///   → AbilityComponent 通知 NotifyModifiersChanged()
    ///   → 全量重算 deniedMask（所有活跃控制的 Deny 并集）
    ///   → 掩码确实变化才广播 MessageId.ControlStateChanged
    ///   → 从"无控制"进入"被控制"时，按 ControlInterrupt 执行一次打断
    /// </code>
    ///
    /// 为什么是掩码而不是类型判断：新增一种控制（定身/沉默/缴械）只需在配置里填 Deny 位，
    /// 任何入口代码都不用改；多个控制并存时取并集，"眩晕 + 减速"天然共存，
    /// 也不需要维护"哪种控制压制哪种"的互斥矩阵。
    ///
    /// 代价与边界：重算在 modifier 增删时做一次全量扫描（O(活跃 modifier 数)），
    /// 而 <see cref="CanDo"/> 是纯字段位与，O(1) —— 高频路径（每帧的移动/转向判定）
    /// 从此不再遍历链表。
    /// </summary>
    public class ControlComponent : Component, IComponent
    {
        private AbilityComponent abilityComponent;

        /// <summary>当前被剥夺的能力位并集。全部活跃控制 Deny 的或。</summary>
        private ControlFlag deniedMask;
        /// <summary>当前最高控制等级，只给表现层用（决定"新来的控制值不值得换表现"），不参与判定。</summary>
        private int topLevel;

        /// <summary>当前被剥夺的能力位并集，O(1) 读取。</summary>
        public ControlFlag DeniedMask => deniedMask;

        /// <summary>
        /// 当前最高控制等级。表现层比较"新来的控制值不值得换动画"时用，
        /// 行为判定不要看它 —— 一律看 <see cref="DeniedMask"/>。
        /// </summary>
        public int TopControlLevel => topLevel;

        /// <summary>
        /// 掩码变化回调（同实体内部组件用，省掉走消息系统的一次查找）。
        /// 跨系统（UI / 模型）请订阅 <see cref="MessageId.ControlStateChanged"/>。
        /// </summary>
        public event Action<ControlFlag, ControlFlag> OnMaskChanged;

        #region IComponent

        public void Init()
        {
            deniedMask = ControlFlag.None;
            topLevel = int.MinValue;
            OnMaskChanged = null;
            // 初始沉默重建：实体创建时可能已经带着控制类 modifier（如出生自带 buff），
            // 但那属于"已有状态"，不该触发打断。
            Rebuild(true);
        }

        public void Destroy()
        {
            OnMaskChanged = null;
            abilityComponent = null;
            deniedMask = ControlFlag.None;
            topLevel = 0;
        }

        #endregion

        /// <summary>
        /// 是否还能执行某项行为。所有行为入口的唯一判定出口。
        /// 注意传单个能力位；需要多个时请分别判断（用 <see cref="ControlFlag.CrowdControl"/>
        /// 这种组合位判断会变成"全部都被禁"才算禁，不是想要的效果）。
        /// </summary>
        public bool CanDo(ControlFlag flag)
        {
            return flag == ControlFlag.None || (deniedMask & flag) == 0;
        }

        /// <summary>
        /// modifier 增删后由 <see cref="AbilityComponent"/> 调用。
        /// 只在此时做全量重算，避免每帧扫描。
        /// </summary>
        public void NotifyModifiersChanged()
        {
            if (IsDispose) return;
            Rebuild(false);
        }

        private void Rebuild(bool silent)
        {
            if (abilityComponent == null || abilityComponent.IsDispose)
                abilityComponent = GetComponent<AbilityComponent>();

            ControlFlag mask = ControlFlag.None;
            int bestLevel = int.MinValue;
            ControlInterrupt interrupt = ControlInterrupt.None;

            var mods = abilityComponent?.Modifiers;
            if (mods != null)
            {
                for (var node = mods.First; node != null; node = node.Next)
                {
                    var modifier = node.Value;
                    if (modifier == null || modifier.IsDispose || modifier.Config == null) continue;

                    ControlFlag deny = modifier.Config.Deny;
                    if (deny == ControlFlag.None) continue;

                    mask |= deny;

                    // 表现层取"等级最高的那条"：同时有眩晕和减速时只播眩晕的表现，
                    // 但两者的 Deny 都已经并入 mask，行为上仍然同时生效。
                    if (modifier.Config.ControlLevel > bestLevel)
                    {
                        bestLevel = modifier.Config.ControlLevel;
                    }

                    if (modifier.Config.Interrupt > interrupt) interrupt = modifier.Config.Interrupt;
                }
            }

            ControlFlag old = deniedMask;
            // 只比掩码：掩码没变就什么都不做（等级变化不单独触发广播，
            // 避免"同一种控制换了个来源"也惊动 UI）。
            if (old == mask) return;

            deniedMask = mask;
            topLevel = bestLevel;

            // 只有"从无控制到被控制"这一次才打断。已处于控制中再叠一层控制不重复打断，
            // 否则连控时会反复把目标的动作重置回起点。
            if (!silent && old == ControlFlag.None && mask != ControlFlag.None)
            {
                ApplyInterrupt(interrupt);
            }

            OnMaskChanged?.Invoke(old, mask);

            if (Messager.Instance != null)
            {
                Messager.Instance.Broadcast(Id, MessageId.ControlStateChanged, new ControlStateEvent
                {
                    Id = Id,
                    OldMask = old,
                    NewMask = mask,
                    Level = bestLevel,
                });
            }
        }

        /// <summary>
        /// 控制生效瞬间的处置：打断技能、按策略处理寻路。
        /// 这里是"打断"的唯一落点 —— 以前 <see cref="CombatComponent.ReleaseSkillImmediately"/>
        /// 只有 AI 的失败分支会调，控制链路从来不调，导致眩晕打不断正在施放的技能。
        /// </summary>
        private void ApplyInterrupt(ControlInterrupt interrupt)
        {
            if (interrupt == ControlInterrupt.None) return;

            GetComponent<CombatComponent>()?.ReleaseSkillImmediately();
        }
    }
}
