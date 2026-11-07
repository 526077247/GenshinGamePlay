namespace TaoTie
{
    /// <summary>
    /// 状态标记监听：订阅 <see cref="MessageId.EntityTagsChanged"/>，满足触发模式时执行 Actions。
    ///
    /// 两个必须守住的约定：
    /// 1. <b>按 ControlComponent.Id（组件 Id）订阅</b>，<see cref="ControlComponent.Rebuild"/>
    ///    用的就是它；按实体 Id 订阅永远收不到事件。组件 Id 在 Destroy 时会被回收改写，
    ///    所以 Init 时先存一份 <see cref="controlId"/>，Dispose 时才能退订对同一个 key；
    /// 2. <b>一次事件最多执行一次 Actions</b>。一击同时挂眩晕+灼烧只发一条事件，
    ///    不要按匹配到的标记个数循环。
    ///
    /// Actions 里 AttachModifier / RemoveModifier 会再次 NotifyModifiersChanged → Rebuild →
    /// 同步嵌套广播。<see cref="Messager"/> 做了监听快照，但回调仍是同步重入的，
    /// 因此用 <see cref="running"/> 拦住：「本 Mixin 的 Actions 引发的二次变化不再触发本 Mixin」。
    /// </summary>
    public class DoActionOnEntityTagsChangeMixin : AbilityMixin<ConfigDoActionOnEntityTagsChangeMixin>
    {
        private ControlComponent control;
        private long controlId;
        private bool running;

        protected override void InitInternal(ActorAbility actorAbility, ActorModifier actorModifier,
            ConfigDoActionOnEntityTagsChangeMixin config)
        {
            var owner = actorAbility.Parent.GetParent<Entity>();
            control = owner?.GetComponent<ControlComponent>();
            if (control == null)
            {
                Log.Warning($"[GameplayTag] {owner?.GetType().Name} 上没有 ControlComponent：标记监听挂载失败。" +
                            "这类实体不聚合状态标记，也永远不会广播 EntityTagsChanged");
                return;
            }

            controlId = control.Id;
            Messager.Instance.AddListener<EntityTagsEvent>(controlId, MessageId.EntityTagsChanged, OnTagsChanged);

            if (config.TriggerOnInit && control.Tags != GameplayTags.None)
            {
                // 用「从 None 变成当前集合」的合成事件复用同一套判定，Added / Level 模式都能命中。
                // 此时读的是挂载前快照：宿主 modifier 自己带的标记还没进 Rebuild。
                OnTagsChanged(new EntityTagsEvent
                {
                    Id = controlId,
                    OldTags = GameplayTags.None,
                    NewTags = control.Tags,
                    StackChanged = false,
                });
            }
        }

        private void OnTagsChanged(EntityTagsEvent evt)
        {
            // 实体销毁后仍可能有一帧残留事件；Dispose 会把 baseConfig / actorAbility 置空
            if (running || Config == null || actorAbility == null) return;

            GameplayTags watch = Config.WatchTags;
            if (watch == GameplayTags.None) return;   // 加载期校验会报错，运行时静默不触发

            bool trigger;
            switch (Config.Mode)
            {
                case TagTriggerMode.Added:
                    trigger = (evt.Added & watch) != GameplayTags.None;
                    break;
                case TagTriggerMode.Removed:
                    trigger = (evt.Removed & watch) != GameplayTags.None;
                    break;
                case TagTriggerMode.StackChanged:
                    // 标记集合没变、只是层数涨落（灼烧 2→3 层）
                    trigger = evt.StackChanged && evt.OldTags == evt.NewTags &&
                              (evt.NewTags & watch) != GameplayTags.None;
                    break;
                case TagTriggerMode.Any:
                    trigger = ((evt.Added | evt.Removed) & watch) != GameplayTags.None ||
                              (evt.StackChanged && (evt.NewTags & watch) != GameplayTags.None);
                    break;
                case TagTriggerMode.Level:
                    trigger = (evt.NewTags & watch) != GameplayTags.None;
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
                Messager.Instance?.RemoveListener<EntityTagsEvent>(controlId, MessageId.EntityTagsChanged, OnTagsChanged);
                controlId = 0;
            }
            control = null;
            running = false;
        }
    }

    /// <summary>
    /// 两个事件监听 Mixin 共用的 Actions 执行器。
    ///
    /// <b>刻意是静态 helper 而不是公共基类</b>：<c>AbilitySystem.GetCreateTypeMap</c> 用
    /// <c>type.BaseType.GenericTypeArguments[0]</c> 反查「配置类型 → 运行时类型」，
    /// 中间插一个非泛型基类会让映射取错、Mixin 创建失败。
    /// </summary>
    internal static class ControlEventMixinHelper
    {
        public static void DoActions(ConfigAbilityPredicate predicate, ConfigAbilityAction[] actions,
            Entity executer, ActorAbility ability, ActorModifier modifier)
        {
            if (actions == null || actions.Length == 0 || executer == null) return;
            if (predicate != null && !predicate.Evaluate(executer, ability, modifier, executer)) return;

            for (int i = 0; i < actions.Length; i++)
            {
                actions[i]?.DoExecute(executer, ability, modifier, executer);
            }
        }
    }
}
