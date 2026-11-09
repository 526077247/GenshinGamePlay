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
    /// <b>本类不认识「次数」这个概念</b>：
    /// 它只回答两件事 —— 「这组标记我免不免」和「免了之后跑哪些 Actions」。
    /// 有限次数、扣减、耗尽移除，全部是配置层用通用件拼出来的：
    /// <code>
    /// ModifierSpecials: { "ImmuneCount": 3 }
    /// Mixins: [
    ///   ConfigImmuneMixin{ ImmuneTags = Stun, Actions = [ AddSpecial(ImmuneCount, -1) ] },
    ///   ConfigDoActionOnSpecialsChangeMixin{ Key = ImmuneCount, Compare = LEqual, Value = 0,
    ///                                        Mode = FallingEdge, Actions = [ RemoveModifier ] }
    /// ]
    /// </code>
    /// 这样 Mixin 不需要知道状态名，也就不存在「key 拼错 → 静默变成永久免疫」这类错；
    /// 而「扣几次」「扣到多少算耗尽」「耗尽后是移除还是只播特效」全在配置里，改行为不用改代码。
    /// 不配 <c>AddSpecial</c> 就是永久免疫，与以前「不配次数」的默认行为一致。
    /// </summary>
    public class ImmuneMixin : AbilityMixin<ConfigImmuneMixin>
    {
        /// <summary>宿主实体的控制仲裁者（免疫注册表所在处）。</summary>
        private ControlComponent host;

        protected override void InitInternal(ActorAbility actorAbility, ActorModifier actorModifier,
            ConfigImmuneMixin config)
        {
            // 无 ControlComponent 的实体（Gadget / Character）拿不到 host，注册静默失败：
            // 这类实体按 ApplyModifier 的语义本来就"全免"，免疫配置是冗余的，不是 bug。
            var context = actorModifier != null ? (BaseActorActionContext)actorModifier : actorAbility;
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

        /// <summary>
        /// 本条是否免疫这组标记（只要命中任一位即算）。
        ///
        /// 不看次数 —— 剩余次数是配置层的事。想让某些条件失效，
        /// 在 Actions 里把状态扣到 0 并用 <c>ConfigDoActionOnSpecialsChangeMixin</c> 移除宿主即可；
        /// 或者给这条 modifier 配 Duration，到期自然注销。
        /// </summary>
        public bool CanImmune(GameplayTags tags)
        {
            return tags != GameplayTags.None
                   && Config != null
                   && (Config.ImmuneTags & tags) != GameplayTags.None;
        }

        /// <summary>
        /// 执行「免疫成功」的 Actions。初始目标是宿主自己，需要指向攻击者请在本条里配 Targetting。
        ///
        /// Actions 里可能移除宿主（扣到 0 → RemoveModifier），
        /// 那条路径走帧末延迟，所以本栈里继续执行剩余的 Actions 是安全的。
        /// </summary>
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
    }
}
