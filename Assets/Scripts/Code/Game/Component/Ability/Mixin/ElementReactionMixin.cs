namespace TaoTie
{
    /// <summary>
    /// 元素反应监听：订阅 <see cref="MessageId.ElementReaction"/>，命中监听的反应 Id 时执行 Actions。
    ///
    /// <b>订阅键是宿主实体 Id</b>（<see cref="ElementComponent.Id"/> 就是实体 Id），
    /// 广播方 ElementComponent 用的也是它 —— 按别的 key 订阅永远收不到。
    ///
    /// <b>重入保护</b>：Actions 里完全可能再挂元素 / 再触发反应（比如「蒸发后追一层火」），
    /// 那会在本次广播的调用栈里再进一次本方法，形成无限递归。用 <see cref="running"/> 拦住：
    /// 本 Mixin 的 Actions 引发的二次反应不再由本 Mixin 处理。
    /// </summary>
    public class ElementReactionMixin : AbilityMixin<ConfigElementReactionMixin>
    {
        private long elementId;
        private bool running;

        protected override void InitInternal(ActorAbility actorAbility, ActorModifier actorModifier,
            ConfigElementReactionMixin config)
        {
            var owner = actorAbility.Parent.GetParent<Entity>();
            var element = owner?.GetComponent<ElementComponent>();
            if (element == null)
            {
                Log.Warning($"[元素反应] {owner?.GetType().Name} 上没有 ElementComponent：反应监听挂载失败。" +
                            "这类实体不会发生元素反应，挂这条 Mixin 没有意义");
                return;
            }

            elementId = element.Id;
            Messager.Instance.AddListener<ElementReactionEvent>(elementId, MessageId.ElementReaction, OnReaction);
        }

        private void OnReaction(ElementReactionEvent evt)
        {
            // 实体销毁后仍可能有一帧残留事件；Dispose 会把 baseConfig / actorAbility 置空
            if (running || Config == null || actorAbility == null) return;
            if (Config.ReactionId != 0 && Config.ReactionId != evt.ReactionId) return;

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
            if (elementId != 0)
            {
                Messager.Instance?.RemoveListener<ElementReactionEvent>(elementId, MessageId.ElementReaction, OnReaction);
                elementId = 0;
            }
            running = false;
        }
    }
}
