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
            if (config.TriggerOnBreak)
            {
                var entity = actor;
                await TimerManager.Instance.WaitAsync((int) ((config.StartTime - nowtime) * 1000));
                if (entity == null || entity.IsDispose) return;
                actor.GetComponent<FsmComponent>()?.OnFsmTimelineTrigger(config.TriggerId);
            }
        }
    }
}