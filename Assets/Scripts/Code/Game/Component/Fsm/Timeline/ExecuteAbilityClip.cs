namespace TaoTie
{
    public class ExecuteAbilityClip:FsmClip<ConfigExecuteAbilityClip>
    {
        protected override void OnStart()
        {
            actor.GetComponent<AbilityComponent>()?.ExecuteAbility(config.AbilityName);
        }

        protected override void OnStop()
        {
            
        }

        protected override void OnUpdate(float nowtime, float elapsetime)
        {
            
        }

        /// <summary>
        /// 状态在 clip 触发前被打断时的补执行。成对 Hide/Show 靠它兜底：
        /// Hide 已写 Renderer.enabled=false / HitBox=false，Show 必须补上，否则永久停在隐藏。
        ///
        /// ExecuteOnBreak 只允许打在「恢复型」clip 上（当前全库唯一一例：
        /// KeQing_NormalAttack4_Show@1.41）。技能本体打 1 会让「取消技能」变成「补放技能本体」，
        /// 那是配置错误，Skill1.playable 已置 0。
        ///
        /// 延迟与否由<b>打断来源</b>决定，见 <see cref="OnBreakAsync"/>：连段内过渡保留到
        /// config.StartTime（美术节奏），硬取消立即执行（不留隐藏尾巴）。
        ///
        /// 调用方在 Break 返回后会立刻 clip.Dispose() 把 state/cfg 置空并回收进对象池，
        /// 所以协程用到的东西必须在同步段全部快照。
        /// </summary>
        protected override void OnBreak(float nowtime)
        {
            base.OnBreak(nowtime);
            OnBreakAsync(nowtime).Coroutine();
        }

        /// <summary>
        /// 被打断时的补执行。延迟与否<b>按「下一个状态还收不收剑」区分</b>：
        ///
        /// • 连段内过渡（NA4 → NA5，下一个状态 ShowWeapon=true）：保持等到 config.StartTime。
        ///   这条延迟是美术节奏本身 —— NA4 在 0.7 秒左右就过渡到 NA5，虚化要一直保持到 1.41 才回显，
        ///   提前执行会让「普攻第 5 段过程中的虚化」整段消失。
        /// • 硬取消（冲刺 / 受击 / 死亡 / 收剑，下一个状态 ShowWeapon=false）：立即执行。
        ///
        /// 但「登记时是连段、等的中途才冲刺」这种情况这里判不出来（那时本状态早已退出、不会再
        /// 产生第二次 Break）。所以延迟走 <see cref="AbilityComponent.RegisterPendingRestore"/>，
        /// 由 <c>EquipHoldComponent</c> 在聚合显隐落到收剑态时调
        /// <see cref="AbilityComponent.FlushPendingRestores"/> 提前兑现。
        /// </summary>
        private async ETTask OnBreakAsync(float nowtime)
        {
            if (config == null || !config.ExecuteOnBreak) return;

            var name = config.AbilityName;
            var entity = actor;
            if (string.IsNullOrEmpty(name) || entity == null || entity.IsDispose) return;

            var abilityComponent = entity.GetComponent<AbilityComponent>();
            if (abilityComponent == null) return;

            // 优先看「本次切换的目标」：FadeDuration==0 时 OnExit 发生在 currentState 赋值之前，
            // 只看 CurrentState 会读到正在退出的状态，判定不出这是冲刺打断。
            var fsm = State?.Fsm;
            var targetName = fsm?.PendingTargetName;
            var nextCfg = targetName != null ? fsm.GetStateConfig(targetName) : fsm?.CurrentState?.Config;
            // Data 为空 ⇒ StateData.ShowWeapon 读作 false ⇒ 同样按收剑态（硬取消）处理。
            bool hardCancel = nextCfg == null || nextCfg.Data == null || !nextCfg.Data.ShowWeapon;

            int waitMs = hardCancel ? 0 : (int) ((config.StartTime - nowtime) * 1000f);
            if (waitMs <= 0)
            {
                abilityComponent.ExecuteAbility(name);
                return;
            }

            var token = abilityComponent.RegisterPendingRestore(name);
            if (token == null)
            {
                abilityComponent.ExecuteAbility(name);
                return;
            }

            // 返回 false 表示已被 FlushPendingRestores 提前兑现（或令牌被重建），这里不要再执行一次。
            if (!await TimerManager.Instance.WaitAsync(waitMs, token)) return;

            abilityComponent.CompletePendingRestore(name, token);
            if (entity.IsDispose) return;
            abilityComponent.ExecuteAbility(name);
        }
    }
}
