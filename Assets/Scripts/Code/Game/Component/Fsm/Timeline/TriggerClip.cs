namespace TaoTie
{
    public class TriggerClip:FsmClip<ConfigTriggerClip>
    {
        protected override void OnStart()
        {
            actor.GetComponent<FsmComponent>()?.OnFsmTimelineTrigger(config.TriggerId);
        }

        protected override void OnStop()
        {
            
        }

        protected override void OnUpdate(float nowtime, float elapsetime)
        {
            
        }

        protected override void OnBreak(float nowtime)
        {
            base.OnBreak(nowtime);
            OnBreakAsync(nowtime).Coroutine();
        }

        private async ETTask OnBreakAsync(float nowtime)
        {
            if (config == null || !config.TriggerOnBreak) return;

            // 同 ExecuteAbilityClip：await 之后 config/actor 已被 clip.Dispose() 清空，
            // 必须在同步段快照；actor 也不能在 await 后再求值（state 已为 null）。
            var triggerId = config.TriggerId;
            var entity = actor;
            if (entity == null || entity.IsDispose) return;

            int delayMs = (int) ((config.StartTime - nowtime) * 1000f);
            if (!await TimerManager.Instance.WaitAsync(delayMs > 0 ? delayMs : 0)) return;
            if (entity.IsDispose) return;

            entity.GetComponent<FsmComponent>()?.OnFsmTimelineTrigger(triggerId);
        }
    }
}