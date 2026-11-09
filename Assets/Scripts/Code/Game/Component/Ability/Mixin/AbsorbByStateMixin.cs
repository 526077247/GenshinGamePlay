using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 伤害吸收的运行时：把宿主的修饰器状态当作伤害池。
    ///
    /// <b>本类不存任何数值</b> —— 池子的大小、回复、破零反应全在 modifier 的状态字典与配置里。
    /// 这里只做两件事：
    /// 1. <see cref="OnBeforeBeAttack"/> 里判断「有没有资格挡这一击」，登记到
    ///    <see cref="AttackResult"/>（此刻还不知道这一击多少伤害，不能扣）；
    /// 2. <see cref="AbsorbDamage"/> 里按实际伤害扣池子，返回真正吸收量。
    ///
    /// 之所以要把「登记」与「扣减」分成两步，见 <see cref="AttackResult.RegisterAbsorb"/> 的注释：
    /// 最终伤害要到 DamageClose 后半段才算出，提前扣只能靠预演公式，而预演暴击会消耗
    /// <c>Random</c>，把真实的暴击序列一起改掉。
    /// </summary>
    public class AbsorbByStateMixin : AbilityMixin<ConfigAbsorbByStateMixin>, IDamageAbsorber
    {
        /// <summary>状态所在的上下文。必须是 modifier —— 见类型注释里的「为什么必须挂在 modifier 上」。</summary>
        private ActorModifier stateHost;

        private CombatComponent host;

        /// <summary>
        /// 配置缓存。运行时不能在 <see cref="AbsorbDamage"/> 里读 <see cref="AbilityMixin{T}.Config"/>：
        /// SetSpecial 之后可能触发监听把宿主移除，那时 Config 已经是空壳。
        /// </summary>
        private string specialKey;
        private int priority;

        public int AbsorbPriority => priority;

        protected override void InitInternal(ActorAbility actorAbility, ActorModifier actorModifier,
            ConfigAbsorbByStateMixin config)
        {
            if (actorModifier == null)
            {
                Log.Error($"ConfigAbsorbByStateMixin 必须挂在修饰器上：ability 级的状态是施法者共享的，" +
                          $"一个技能给多个目标加盾会共用同一份池子；Ability = {actorAbility?.Config?.AbilityName}");
                return;
            }

            if (string.IsNullOrEmpty(config.SpecialKey))
            {
                Log.Error($"ConfigAbsorbByStateMixin 的 SpecialKey 为空，不会吸收任何伤害；" +
                          $"Ability = {actorAbility?.Config?.AbilityName}");
                return;
            }

            var entity = GetActionExecuter();
            if (entity == null)
            {
                Log.Error($"ConfigAbsorbByStateMixin 初始化时拿不到宿主实体，吸收不会生效；" +
                          $"Ability = {actorAbility?.Config?.AbilityName}");
                return;
            }

            if (!actorModifier.ContainsSpecial(config.SpecialKey))
            {
                // 不拦：SetSpecial 对不存在的 key 会自动 Add，盾照样能工作（初值 0 除外）。
                // 但「初值是 0 + 没有 tick 回盾」= 永远不挡伤害，这种静默失效最难查。
                Log.Warning($"ConfigAbsorbByStateMixin 引用的状态 {config.SpecialKey} 没有初值；" +
                            $"检查 ConfigAbilityModifier.ModifierSpecials 是否漏配；" +
                            $"Ability = {actorAbility?.Config?.AbilityName}");
            }

            stateHost = actorModifier;
            specialKey = config.SpecialKey;
            priority = config.Priority;

            host = entity.GetComponent<CombatComponent>();
            if (host != null)
            {
                host.beforeBeAttack += OnBeforeBeAttack;
            }
            else
            {
                // 没有 CombatComponent 的实体根本不会走 DamageClose，配了也是死配置
                Log.Warning($"ConfigAbsorbByStateMixin 挂在了没有 CombatComponent 的实体 ({entity.Id}) 上，" +
                            $"不会吸收任何伤害；Ability = {actorAbility?.Config?.AbilityName}");
            }
        }

        /// <summary>受击前钩子：只做资格判定与登记，绝不在这里扣池子。</summary>
        private void OnBeforeBeAttack(AttackResult result, CombatComponent attacker)
        {
            var config = Config;
            if (config == null || stateHost == null || stateHost.IsDispose) return;

            if (!AttackHelper.MatchAttackTag(config.MatchAttackTag, result)) return;

            // 池子空了就别登记，省掉一次排序与回调
            if (stateHost.GetSpecial(specialKey) <= 0f) return;

            result.RegisterAbsorb(this, priority);
        }

        /// <summary>
        /// 由 <see cref="AttackResult.ResolveAbsorb"/> 在最终伤害算出后调用。
        /// 返回实际吸收量；可能小于 <paramref name="damage"/>（池子不够厚）。
        /// </summary>
        public int AbsorbDamage(int damage)
        {
            if (damage <= 0) return 0;

            var ctx = stateHost;
            if (ctx == null || ctx.IsDispose) return 0;

            var cur = ctx.GetSpecial(specialKey);
            if (cur <= 0f) return 0;

            // 向上取整：池子是 float，直接 (int) 截断会让剩 0.5 的盾每次吃 0 点、
            // 既不挡伤害也永远不破零，表现成「盾条空了但人还在挨打」。
            int available = Mathf.CeilToInt(cur);
            int take = damage < available ? damage : available;

            var next = cur - take;
            if (next < 0f) next = 0f;

            // SetSpecial 会同步触发 onSpecialChange：监听方可能在回调里移除本修饰器
            // （RemoveModifier 走帧末延迟，所以这里安全）。之后不得再访问 this 的任何成员。
            ctx.SetSpecial(specialKey, next);

            return take;
        }

        protected override void DisposeInternal()
        {
            if (host != null)
            {
                host.beforeBeAttack -= OnBeforeBeAttack;
                host = null;
            }

            // 状态不在这里清：它属于 modifier，由 modifier 的 Dispose 统一回收。
            // 本类只是「读取者」，清它会让同一个 modifier 上的其他 Mixin 读到 0。
            stateHost = null;
            specialKey = null;
            priority = 0;
        }
    }
}
