namespace TaoTie
{
    public class AttachAbilityAction:FsmClip<ConfigAttachAbility>
    {
        public static AttachAbilityAction Create()
        {
            return ObjectPool.Instance.Fetch<AttachAbilityAction>();
        }
        protected override void OnStart()
        {
            var conf = ConfigAbilityCategory.Instance.Get(config.AbilityName);
            actor.GetComponent<AbilityComponent>().AddAbility(conf);
        }

        protected override void OnStop()
        {
            if (config.RemoveOnOver)
                actor.GetComponent<AbilityComponent>().RemoveAbility(config.AbilityName);
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
            if (config.AddOnBreak)
            {
                var entity = actor;
                await TimerManager.Instance.WaitAsync((int) ((config.StartTime - nowtime) * 1000));
                if (entity == null || entity.IsDispose) return;
                var conf = ConfigAbilityCategory.Instance.Get(config.AbilityName);
                var ability = entity.GetComponent<AbilityComponent>()?.AddAbility(conf);
                if (config.RemoveOnOver)
                {
                    entity.GetComponent<AbilityComponent>()?.RemoveAbility(ability);
                }
            }
        }
    }
}