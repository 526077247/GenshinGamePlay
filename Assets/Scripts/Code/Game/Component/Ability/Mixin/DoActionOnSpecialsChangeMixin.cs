using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 修饰器状态监听：订阅宿主上下文的 <see cref="BaseActorActionContext.onSpecialChange"/>，
    /// 越过阈值时执行 Actions。
    ///
    /// 挂在 modifier 上监控的是「这个目标这一份」的状态，挂在 ability 上监控的是
    /// 施法者自己的 Specials —— 两者共用同一套字典（见 <see cref="BaseActorActionContext"/>）。
    /// 需要「每个人各自一份」的机制（护盾）必须挂在 modifier 上，否则多个目标会读同一份。
    /// </summary>
    public class DoActionOnSpecialsChangeMixin : AbilityMixin<ConfigDoActionOnSpecialsChangeMixin>
    {
        private BaseActorActionContext context;

        /// <summary>
        /// 重入保护。Actions 里完全可能再写同一个 key（比如「充能满 → 扣掉 10 点换取一次效果」），
        /// 那会同步触发本监听、再执行一次 Actions，形成无限递归。
        /// 递归一次就足够表达意图，多余的嵌套一律丢弃。
        /// </summary>
        private bool executing;

        protected override void InitInternal(ActorAbility actorAbility, ActorModifier actorModifier,
            ConfigDoActionOnSpecialsChangeMixin config)
        {
            context = actorModifier != null ? (BaseActorActionContext)actorModifier : actorAbility;

            if (context == null || context.IsDispose)
            {
                Log.Error($"[Special监听] 挂载失败：没有可用的执行上下文；Ability = {actorAbility?.Config?.AbilityName}");
                return;
            }

            if (string.IsNullOrEmpty(config.Key))
            {
                Log.Error($"[Special监听] Key 为空，监听不会生效；Ability = {actorAbility?.Config?.AbilityName}");
                return;
            }

            if (!context.ContainsSpecial(config.Key))
            {
                // 不拦，但要吵：这是「配了监听但初值表里没有这个 key」，
                // 运行时 Set 会自动 Add，所以最终能工作 —— 但初值就是 0，
                // 「跨入 0」永远不会发生，表现成静默失效。
                Log.Warning($"[Special监听] 上下文里没有状态 {config.Key}，初值按 0 处理；" +
                            $"检查 ModifierSpecials / AbilitySpecials 是否漏配；Ability = {actorAbility?.Config?.AbilityName}");
            }

            context.onSpecialChange += OnSpecialChange;

            // 初值在 ActorModifier.Create 里、创建 Mixin 之前就已写入，所以这里读得到真实初值
            if (config.TriggerOnInit && CompareValue(context.GetSpecial(config.Key)))
            {
                DoActions();
            }
        }

        private void OnSpecialChange(string key, float newValue, float oldValue)
        {
            if (context == null || Config == null || key != Config.Key) return;

            bool matchOld = CompareValue(oldValue);
            bool matchNew = CompareValue(newValue);

            bool trigger;
            switch (Config.Mode)
            {
                case NumericTriggerMode.Level:
                    trigger = matchNew;
                    break;
                case NumericTriggerMode.FallingEdge:
                    // 「跨入」而非「本来就在里面」：已知旧值不满足、新值满足，只在这一刻成立
                    trigger = !matchOld && matchNew;
                    break;
                case NumericTriggerMode.RisingEdge:
                    trigger = matchOld && !matchNew;
                    break;
                case NumericTriggerMode.AnyChange:
                    trigger = true;
                    break;
                default:
                    trigger = false;
                    break;
            }

            if (trigger) DoActions();
        }

        private bool CompareValue(float value)
        {
            float threshold = Config.Value;
            switch (Config.Compare)
            {
                case CompareMode.Equal:
                    return Mathf.Approximately(value, threshold);
                case CompareMode.NotEqual:
                    return !Mathf.Approximately(value, threshold);
                case CompareMode.Greater:
                    return value > threshold;
                case CompareMode.Less:
                    return value < threshold;
                case CompareMode.LEqual:
                    return value <= threshold;
                case CompareMode.GEqual:
                    return value >= threshold;
                default:
                    return false;
            }
        }

        private void DoActions()
        {
            if (executing) return;

            var actions = Config.Actions;
            if (actions == null || actions.Length == 0) return;

            var executer = GetActionExecuter();
            if (Config.Predicate != null && !Config.Predicate.Evaluate(executer, actorAbility, actorModifier, executer))
            {
                return;
            }

            executing = true;
            try
            {
                for (int i = 0; i < actions.Length; i++)
                {
                    // 这里传的是「状态持有者」而不是 actionExecuter 的副本：
                    // 对 modifier 级监听两者相同；对 ability 级监听 actionExecuter 也是施法者。
                    actions[i].DoExecute(executer, actorAbility, actorModifier, executer);
                }
            }
            finally
            {
                executing = false;
            }
        }

        protected override void DisposeInternal()
        {
            if (context != null)
            {
                context.onSpecialChange -= OnSpecialChange;
                context = null;
            }

            executing = false;
        }
    }
}
