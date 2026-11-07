namespace TaoTie
{
    /// <summary>
    /// 控制状态监听：订阅 <see cref="MessageId.ControlStateChanged"/>，满足触发模式时执行 Actions。
    ///
    /// 这一轴是「结论」（能力被剥夺成什么样），配置面没有按能力位过滤的字段 ——
    /// <see cref="ControlFlag"/> 是引擎内部枚举，不出现在配置里。
    /// 想按原因（眩晕 / 冰冻）过滤请用 <see cref="DoActionOnEntityTagsChangeMixin"/>。
    ///
    /// <b>Begin 走 <see cref="ControlStateEvent.Gained"/> 而不是 IsBegin</b>：
    /// 吟唱中被眩晕时 OldMask 已非 None，IsBegin 为 false，但 Gained 会拿到新剥夺的位。
    /// 另外 <see cref="ControlComponent.Rebuild"/> 用 <c>old == deny</c> 门控广播，
    /// 两个控制派生出相同剥夺并集时第二个不广播 —— 这是结论轴的固有边界，别拿它计次数。
    /// </summary>
    public class DoActionOnControlStateChangeMixin : AbilityMixin<ConfigDoActionOnControlStateChangeMixin>
    {
        private ControlComponent control;
        private long controlId;
        private bool running;

        protected override void InitInternal(ActorAbility actorAbility, ActorModifier actorModifier,
            ConfigDoActionOnControlStateChangeMixin config)
        {
            var owner = actorAbility.Parent.GetParent<Entity>();
            control = owner?.GetComponent<ControlComponent>();
            if (control == null)
            {
                Log.Warning($"[GameplayTag] {owner?.GetType().Name} 上没有 ControlComponent：控制状态监听挂载失败。" +
                            "这类实体不聚合状态标记，也永远不会广播 ControlStateChanged");
                return;
            }

            controlId = control.Id;
            Messager.Instance.AddListener<ControlStateEvent>(controlId, MessageId.ControlStateChanged, OnControlStateChanged);

            if (config.TriggerOnInit && control.DeniedMask != ControlFlag.None)
            {
                // 合成「从全无到当前剥夺集合」的事件，复用同一套判定。
                OnControlStateChanged(new ControlStateEvent
                {
                    Id = controlId,
                    OldMask = ControlFlag.None,
                    NewMask = control.DeniedMask,
                    Level = control.TopControlLevel,
                });
            }
        }

        private void OnControlStateChanged(ControlStateEvent evt)
        {
            // 实体销毁后仍可能有一帧残留事件；Dispose 会把 baseConfig / actorAbility 置空
            if (running || Config == null || actorAbility == null) return;

            bool trigger;
            switch (Config.Mode)
            {
                case ControlTriggerMode.Begin:
                    trigger = evt.Gained != ControlFlag.None;
                    break;
                case ControlTriggerMode.End:
                    trigger = evt.IsEnd;
                    break;
                case ControlTriggerMode.HardStop:
                    // 「本次进入完全钉死」：已在硬控里再叠一个软控不重复触发
                    trigger = evt.Gained != ControlFlag.None &&
                              (evt.NewMask & GameplayTagMeta.HardStop) == GameplayTagMeta.HardStop &&
                              (evt.OldMask & GameplayTagMeta.HardStop) != GameplayTagMeta.HardStop;
                    break;
                case ControlTriggerMode.Any:
                    trigger = evt.Gained != ControlFlag.None || evt.Lost != ControlFlag.None;
                    break;
                default:
                    trigger = false;
                    break;
            }

            if (!trigger) return;

            running = true;
            try
            {
                ControlEventMixinHelper.DoActions(Config.Predicate, Config.Actions,
                    GetActionExecuter(), actorAbility, actorModifier);
            }
            finally
            {
                running = false;
            }
        }

        protected override void DisposeInternal()
        {
            if (controlId != 0)
            {
                Messager.Instance?.RemoveListener<ControlStateEvent>(controlId, MessageId.ControlStateChanged, OnControlStateChanged);
                controlId = 0;
            }
            control = null;
            running = false;
        }
    }
}
