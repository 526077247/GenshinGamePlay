namespace TaoTie
{
    /// <summary>
    /// 免疫标记的运行时实现：把自己注册进宿主实体的 <see cref="ControlComponent"/>，
    /// 供 <see cref="ControlComponent.TryGetImmune"/> 在施加带标记的 Modifier 前点查。
    ///
    /// <b>为什么注册到 ControlComponent 而不是 AbilityComponent</b>：
    /// 控制与免疫是同一件事的两面，仲裁者只有一个 —— ControlComponent 既是标记的唯一聚合者，
    /// 也是免疫源的唯一持有者；AbilityComponent 只负责"要不要创建这个实例"。
    ///
    /// <b>为什么用注册表而不是每次去扫 modifier 链表</b>：
    /// 免疫源永远只有 0~2 条（而且大多数实体是 0 条），遍历一个小 List 比遍历
    /// 全部 modifier 再逐条翻它的 Mixins 快，也不依赖 Mixin 的存储结构。
    /// 注册/注销挂在宿主的 afterAdd / beforeRemove 上，跟着生命周期走，不会漏。
    ///
    /// <b>次数为什么存在实例上</b>：<see cref="ConfigImmuneMixin"/> 是全局共享的配置模板，
    /// 扣它等于扣所有实体的免疫。剩余次数属于这一个实例，与 <see cref="ActorModifier.StackCount"/> 同构。
    /// </summary>
    public class ImmuneMixin : AbilityMixin<ConfigImmuneMixin>
    {
        /// <summary>剩余可抵挡次数。不限次时恒为初始值，不参与递减。</summary>
        private int remaining;

        /// <summary>宿主实体的控制仲裁者（免疫注册表所在处）。</summary>
        private ControlComponent host;

        /// <summary>是否不限次（<see cref="ConfigImmuneMixin.ImmuneCount"/> 为 0）。</summary>
        public bool Unlimited => Config != null && Config.ImmuneCount <= 0;

        /// <summary>剩余次数；不限次返回 -1（给 UI 区分"∞"和"还剩 0 次"）。</summary>
        public int Remaining => Unlimited ? -1 : remaining;

        protected override void InitInternal(ActorAbility actorAbility, ActorModifier actorModifier,
            ConfigImmuneMixin config)
        {
            // 次数必须在 Init 里赋值，不能挪到 Register（afterAdd）——
            // afterAdd 是事件，将来任何"重新触发一次 afterAdd"的改动都会把次数刷满，
            // 而那种改动看起来完全无害，且不会立刻暴露。
            remaining = config.ImmuneCount;

            var context = actorModifier != null ? (BaseActorActionContext)actorModifier : actorAbility;
            // 无 ControlComponent 的实体（Gadget / Character）拿不到 host，注册静默失败：
            // 这类实体按 ApplyModifier 的语义本来就"全免"，免疫配置是冗余的，不是 bug。
            host = context?.Parent?.GetComponent<ControlComponent>();

            if (actorModifier != null)
            {
                actorModifier.afterAdd += Register;
                actorModifier.beforeRemove += Unregister;
            }
            else if (actorAbility != null)
            {
                actorAbility.afterAdd += Register;
                actorAbility.beforeRemove += Unregister;
            }
        }

        protected override void DisposeInternal()
        {
            if (actorModifier != null)
            {
                actorModifier.afterAdd -= Register;
                actorModifier.beforeRemove -= Unregister;
            }
            else if (actorAbility != null)
            {
                actorAbility.afterAdd -= Register;
                actorAbility.beforeRemove -= Unregister;
            }

            // 双保险：正常路径由 beforeRemove 注销，这里兜"没走 beforeRemove 就 Dispose"的情况。
            // 重复注销无害（List.Remove 找不到就返回 false）。
            host?.UnregisterImmune(this);
            host = null;
        }

        private void Register()
        {
            host?.RegisterImmune(this);
        }

        private void Unregister()
        {
            host?.UnregisterImmune(this);
        }

        /// <summary>本条是否免疫这组标记（只要命中任一位即算）。</summary>
        public bool CanImmune(GameplayTags tags)
        {
            return tags != GameplayTags.None
                   && Config != null
                   && (Config.ImmuneTags & tags) != GameplayTags.None;
        }

        /// <summary>
        /// 消耗一次免疫。返回 true 表示还能继续挡，false 表示这是最后一次、宿主该移除了。
        /// 不限次永远返回 true 且不递减。
        /// </summary>
        public bool Consume()
        {
            if (Config == null || Config.ImmuneCount <= 0) return true;
            return --remaining > 0;
        }

        /// <summary>执行「免疫成功」的 Actions。必须在宿主被移除 <b>之前</b>调用。</summary>
        public void ExecuteOnImmune()
        {
            var actions = Config?.Actions;
            if (actions == null || actions.Length == 0) return;

            var executer = GetActionExecuter();
            if (executer == null) return;

            for (int i = 0; i < actions.Length; i++)
            {
                actions[i].DoExecute(executer, actorAbility, actorModifier, executer);
            }
        }

        /// <summary>
        /// 次数耗尽后移除宿主（modifier 或 ability）。
        /// <b>调用后本实例已被回收进对象池，不得再访问任何成员</b>。
        /// </summary>
        public void RemoveHost()
        {
            if (actorModifier != null)
            {
                actorModifier.Dispose();
                return;
            }

            actorAbility?.Dispose();
        }
    }
}
