namespace TaoTie
{
    public class AttachAbilityClip:FsmClip<ConfigAttachAbilityClip>
    {
        protected override void OnStart()
        {
            var conf = ConfigAbilityCategory.Instance.Get(config.AbilityName);
            actor.GetComponent<AbilityComponent>()?.AddAbility(conf);
        }

        protected override void OnStop()
        {
            if (config.RemoveOnOver)
                actor.GetComponent<AbilityComponent>()?.RemoveAbility(config.AbilityName);
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
            if (config == null || !config.AddOnBreak) return;

            // Break 返回后调用方会立刻 clip.Dispose()，把 state/cfg 置空并回收进对象池；
            // 协程用到的一切必须在同步段快照，await 之后不能再碰 config / actor。
            var name = config.AbilityName;
            bool removeOnOver = config.RemoveOnOver;
            var entity = actor;
            if (string.IsNullOrEmpty(name) || entity == null || entity.IsDispose) return;

            int delayMs = (int) ((config.StartTime - nowtime) * 1000f);
            if (!await TimerManager.Instance.WaitAsync(delayMs > 0 ? delayMs : 0)) return;
            if (entity.IsDispose) return;

            var conf = ConfigAbilityCategory.Instance.Get(name);
            var ability = entity.GetComponent<AbilityComponent>()?.AddAbility(conf);
            if (removeOnOver)
            {
                entity.GetComponent<AbilityComponent>()?.RemoveAbility(ability);
            }
        }
    }
}