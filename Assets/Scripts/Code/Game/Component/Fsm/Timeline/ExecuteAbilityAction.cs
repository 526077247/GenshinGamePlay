namespace TaoTie
{
    public class ExecuteAbilityAction:FsmClip<ConfigExecuteAbility>
    {
        public static ExecuteAbilityAction Create()
        {
            return ObjectPool.Instance.Fetch<ExecuteAbilityAction>();
        }
        protected override void OnStart()
        {
            actor.GetComponent<AbilityComponent>().ExecuteAbility(config.AbilityName);
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
            if (config.ExecuteOnBreak)
            {
                var name = config.AbilityName;
                var entity = actor;
                await TimerManager.Instance.WaitAsync((int) ((config.StartTime - nowtime) * 1000));
                entity.GetComponent<AbilityComponent>().ExecuteAbility(name);
            }
        }
    }
}